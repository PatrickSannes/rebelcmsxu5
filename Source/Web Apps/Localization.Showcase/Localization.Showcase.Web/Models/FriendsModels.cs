using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using Umbraco.Framework.Localization.Web.Mvc;

namespace Localization.Showcase.Web.Models
{
    public class Friend
    {
        public int ID { get; set; }
        public string Name { get; set; }

        public static List<Friend> All
        {
            get
            {
                return new List<Friend>
                {
                    new Friend { ID = 1, Name = "John" },
                    new Friend { ID = 2, Name = "Abe" },
                    new Friend { ID = 3, Name = "Laurence" },
                    new Friend { ID = 4, Name = "Amanda" },
                    new Friend { ID = 5, Name = "Nick" },
                    new Friend { ID = 6, Name = "Zoe" }
                };
            }
        }
    }

    public class FriendSettings
    {
        public static int RequiredFriends = 3;
    }

    
    public class FriendsChooseModel
    {        

        public List<Friend> Friends { get; set; }

        public List<Friend> AddedFriends { get; set; }
      
        [Display(Name="Labels.ChosenFriends")]
        public Dictionary<int, bool> ChosenFriends { get; set; }

        [Required]
        public string RequiredString { get; set; }

        [Required, LocalizedValidation(typeof(RequiredAttribute), "CustomRequired")]
        public string CustomRequiredString { get; set; }

        [Range(10, 20), LocalizedValidation(typeof(RangeAttribute), "CustomRange")]
        public int CustomRangeInt { get; set; }
    }
}