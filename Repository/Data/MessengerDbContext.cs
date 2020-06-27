﻿using Microsoft.EntityFrameworkCore;
using Repository.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Repository.Data
{
    public class MessengerDbContext : DbContext
    {
        public MessengerDbContext(DbContextOptions<MessengerDbContext>options):base(options)
        {
        }

        public DbSet<Account> Accounts { get; set; }
        public DbSet<AccountDetail> AccountDetails { get; set; }
        public DbSet<AccountFavMessages> AccountFavMessages { get; set; }
        public DbSet<AccountHubs> AccountHubs { get; set; }
        public DbSet<AccountPrivacy> AccountPrivacies { get; set; }
        public DbSet<AccountSecurity> AccountSecurities { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<Hub> Hubs { get; set; }
        public DbSet<HubFile> HubFiles { get; set; }
        public DbSet<Message> Messages { get; set; }

    }
}