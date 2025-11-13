using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using QuanLyKhachSan.Models;

namespace QuanLyKhachSan.Areas.Admin.Validations
{
    public class CustomerValidation
    {
        private List<string> Errors = new List<string>();
        private List<string> Fields;

        FormCollection Collection;

        public CustomerValidation(FormCollection collection)
        {
            this.Collection = collection;

            this.Fields = new List<string>()
            {
                "email", 
                "name",
                "password"
            };
        }

        public List<string> Validation()
        {
            foreach (string field in this.Fields)
            {
                string validation = Collection[field];

                if (string.IsNullOrEmpty(validation))
                {
                    Errors.Add(field + " is Empty");
                }
            }

            return Errors;
        }
    }
}