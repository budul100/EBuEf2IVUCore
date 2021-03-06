﻿using DatabaseConnector.Models;
using Microsoft.EntityFrameworkCore;

namespace DatabaseConnector.Contexts
{
    internal class SitzungContext
        : BaseContext
    {
        #region Public Constructors

        public SitzungContext(string connectionString)
            : base(connectionString)
        { }

        #endregion Public Constructors

        #region Public Properties

        public DbSet<Sitzung> Sitzungen { get; set; }

        #endregion Public Properties
    }
}