﻿using System;
using System.Collections.Generic;
using System.IO;
using LaunchBot.Models;
using SQLite;

namespace LaunchBot.Helpers
{
    public static class DBHelper
    {
        private const string FileName = "database.db";

        /// <summary>
        /// SQLiteConnection has to be opened while you use selected data
        /// </summary>
        /// <returns></returns>
        public static SQLiteConnection GetConnection()
        {
            return new SQLiteConnection(FileName);
        }

        public static void AddUser(Telegram.Bot.Types.User telegramUser)
        {
            using (var db = GetConnection())
            {
                if (db.Find<User>(telegramUser.Id) == null)
                {
                    db.Insert(new User()
                    {
                        Id = telegramUser.Id,
                        UserName = telegramUser.Username,
                        LastName = telegramUser.LastName,
                        FirstName = telegramUser.FirstName
                    });
                }
            }
        }
        public static void UserPassedTheBlock(int id, Block block)
        {
            using (var db = GetConnection())
            {
                var user = db.Find<User>(id);
                switch (block)
                {
                    case Block.Lamagna:{ user.LamagnaPassed = true; break; }
                    case Block.Trippier:{ user.TrippierPassed = true; break; }
                    case Block.MainProduct:{ user.MainProductPassed = true; break; }
                }
                db.Update(user);
            }
        }

        public static void CheckDB()
        {
            if (!DBExists())
            {
                CreateDB();
            }
        }

        public static void CreateDB()
        {
            using (var db = GetConnection())
            {
                db.CreateTable<User>();
            }
        }

        public static bool DBExists()
        {
            return File.Exists(FileName);
        }
    }
}
