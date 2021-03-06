﻿using System;
using System.Collections.Generic;

namespace Split.Models
{
    public partial class Category
    {
        public Category()
        {
            Transaction = new HashSet<Transaction>();
            TransactionParty = new HashSet<TransactionParty>();
        }

        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime ModifiedOn { get; set; }
        public Guid UserId { get; set; }
        public int CategoryType { get; set; }

        public User User { get; set; }
        public ICollection<Transaction> Transaction { get; set; }
        public ICollection<TransactionParty> TransactionParty { get; set; }
    }

    public enum CategoryType
    {
      Expense,
      Income,
      Transfer
    }
}
