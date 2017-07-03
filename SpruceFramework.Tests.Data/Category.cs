﻿// #region Author Information
// // Category.cs
// // 
// // (c) Apexol Technologies. All Rights Reserved.
// // 
// #endregion

using System.ComponentModel.DataAnnotations;

namespace SpruceFramework.Tests.Data
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        public string CategoryName { get; set; }
    }
}