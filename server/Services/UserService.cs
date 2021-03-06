using System;
using System.Collections.Generic;
using System.Linq;
using Split.Models;
using Split.Helpers;

namespace Split.Services
{
  public interface IUserService
  {
    User Authenticate(string email, string password);
    IEnumerable<User> GetAll();
    User GetById(Guid id);
    User GetByEmail(string email);
    User Create(User user, string password);
    void Update(User user, string password = null);
    void Delete(Guid id);
  }

  public class UserService : IUserService
  {
    private SplitContext _context;

    public UserService(SplitContext context)
    {
      _context = context;
    }

    public User Authenticate(string email, string password)
    {
      if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        return null;

      var user = _context.User.SingleOrDefault(x => x.Email == email);

      // check if user email exists
      if (user == null)
        return null;

      // check if password is correct
      if (!VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
        return null;

      // authentication successful
      return user;
    }

    public IEnumerable<User> GetAll()
    {
      return _context.User;
    }

    public User GetById(Guid id)
    {
      return _context.User.Find(id);
    }

    public User GetByEmail(string email)
    {
      return _context.User.Where(u => u.Email.Equals(email)).Single();
    }

    public User Create(User user, string password)
    {
      // validation
      if (string.IsNullOrWhiteSpace(password))
        throw new AppException("Password is required");

      if (_context.User.Any(x => x.Email == user.Email))
        throw new AppException("Email \"" + user.Email + "\" is already taken");

      if (_context.User.Any(x => x.Email == user.Email))
        throw new AppException("Email \"" + user.Email + "\" is already in use by another account.");

      byte[] passwordHash, passwordSalt;
      CreatePasswordHash(password, out passwordHash, out passwordSalt);

      user.PasswordHash = passwordHash;
      user.PasswordSalt = passwordSalt;

      _context.User.Add(user);
      _context.SaveChanges();

      return user;
    }

    public void Update(User userParam, string password = null)
    {
      var user = _context.User.Find(userParam.UserId);

      if (user == null)
        throw new AppException("User not found");

      if (userParam.Email != user.Email)
      {
        // Email has changed so check if the new email is already taken
        if (_context.User.Any(x => x.Email == userParam.Email))
          throw new AppException("Email " + userParam.Email + " is already taken");
      }

      // update user properties
      user.FirstName = userParam.FirstName;
      user.LastName = userParam.LastName;
      user.Email = userParam.Email;

      // update password if it was entered
      if (!string.IsNullOrWhiteSpace(password))
      {
        byte[] passwordHash, passwordSalt;
        CreatePasswordHash(password, out passwordHash, out passwordSalt);

        user.PasswordHash = passwordHash;
        user.PasswordSalt = passwordSalt;
      }

      _context.User.Update(user);
      _context.SaveChanges();
    }

    public void Delete(Guid id)
    {
      var user = _context.User.Find(id);
      if (user != null)
      {
        _context.User.Remove(user);
        _context.SaveChanges();
      }
    }

    // private helper methods

    private static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
    {
      if (password == null) throw new ArgumentNullException("password");
      if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Value cannot be empty or whitespace only string.", "password");

      using (var hmac = new System.Security.Cryptography.HMACSHA512())
      {
        passwordSalt = hmac.Key;
        passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
      }
    }

    private static bool VerifyPasswordHash(string password, byte[] storedHash, byte[] storedSalt)
    {
      if (password == null) throw new ArgumentNullException("password");
      if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Value cannot be empty or whitespace only string.", "password");
      if (storedHash.Length != 64) throw new ArgumentException("Invalid length of password hash (64 bytes expected).", "passwordHash");
      if (storedSalt.Length != 128) throw new ArgumentException("Invalid length of password salt (128 bytes expected).", "passwordHash");

      using (var hmac = new System.Security.Cryptography.HMACSHA512(storedSalt))
      {
        var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
        for (int i = 0; i < computedHash.Length; i++)
        {
          if (computedHash[i] != storedHash[i]) return false;
        }
      }

      return true;
    }
  }
}