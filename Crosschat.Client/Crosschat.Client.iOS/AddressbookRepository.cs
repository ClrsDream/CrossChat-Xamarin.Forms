﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Crosschat.Client.iOS;
using Crosschat.Client.Model;
using MonoTouch.AddressBook;
using MonoTouch.Foundation;
using Xamarin.Forms;

[assembly: Dependency(typeof(AddressbookRepository))]

namespace Crosschat.Client.iOS
{
    public class AddressbookRepository : IAddressbookRepository
    {
        public Task<Contact[]> GetAllAsync()
        {
            var taskCompletionSource = new TaskCompletionSource<Contact[]>();

            NSError err;
            var ab = ABAddressBook.Create(out err);
            if (err != null)
            {
                // process error
                return Task.FromResult(new Contact[0]);
            }
            // if the app was not authorized then we need to ask permission
            if (ABAddressBook.GetAuthorizationStatus() != ABAuthorizationStatus.Authorized)
            {
                ab.RequestAccess(delegate(bool granted, NSError error)
                {
                    if (error != null)
                    {
                        // process error
                    }
                    else if (granted)
                    {
                        Task.Run(() => taskCompletionSource.TrySetResult(GetContacts(ab)));
                    }
                });
            }
            else
            {
                Task.Run(() => taskCompletionSource.TrySetResult(GetContacts(ab)));
            } 

            return taskCompletionSource.Task;
        }

        private Contact[] GetContacts(ABAddressBook ab)
        {
            var contacts = new List<Contact>();
            foreach (var person in ab.GetPeople())
            {
                if (string.IsNullOrEmpty(person.FirstName) && string.IsNullOrEmpty(person.LastName))
                    continue;

                string displayName = string.Format("{0} {1}", person.FirstName, person.LastName);
                string number;
                var phones = person.GetPhones();
                if (phones == null || !phones.Any())
                    continue;

                number = phones[0].Value;
                if (!string.IsNullOrEmpty(number))
                {
                    contacts.Add(new Contact { Name = displayName, Number = number });   
                }
            }
            return contacts.ToArray();
        }
    }
}
