using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ContactApp.Models;

using RhoconnectNET;
using RhoconnectNET.Controllers;

namespace ContactsApp.Controllers
{ 
    public class ContactController : Controller, IRhoconnectCRUD
    {
        private ContactDBContext db = new ContactDBContext();

        //
        // GET: /Contact/

        public ViewResult Index()
        {
            return View(db.Contacts.ToList());
        }

        //
        // GET: /Contact/Details/5

        public ViewResult Details(int id)
        {
            Contact contact = db.Contacts.Find(id);
            return View(contact);
        }

        //
        // GET: /Contact/Create

        public ActionResult Create()
        {
            return View();
        }

        // This method is used to access current partition
        // in Rhoconnect notification callbacks	
        private String partition()
        {
			// If you're using 'app' partition
			// uncomment the following line
			// return "app";
            return "testuser";
        }

        [HttpPost]
        public ActionResult Create(Contact contact)
        {
            if (ModelState.IsValid)
            {
                db.Contacts.Add(contact);
                db.SaveChanges();

                // insert these lines to provide
                // notifications to Rhoconnect server
                RhoconnectNET.Client.notify_on_create(partition(), contact);

                return RedirectToAction("Index");
            }

            return View(contact);
        }
        
        //
        // GET: /Contact/Edit/5
 
        public ActionResult Edit(int id)
        {
            Contact contact = db.Contacts.Find(id);
            return View(contact);
        }

        //
        // POST: /Contact/Edit/5

        [HttpPost]
        public ActionResult Edit(Contact contact)
        {
            if (ModelState.IsValid)
            {
                db.Entry(contact).State = EntityState.Modified;
                db.SaveChanges();

                // insert this callback to notify Rhoconnect
                // about the update operation
                RhoconnectNET.Client.notify_on_update(partition(), contact);

                return RedirectToAction("Index");
            }
            return View(contact);
        }

        //
        // GET: /Contact/Delete/5
 
        public ActionResult Delete(int id)
        {
            Contact contact = db.Contacts.Find(id);
            return View(contact);
        }

        //
        // POST: /Contact/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            Contact contact = db.Contacts.Find(id);
            db.Contacts.Remove(contact);
            db.SaveChanges();

            // insert this callback to notify Rhoconnect
            // about the delete operation
            RhoconnectNET.Client.notify_on_delete("Contact", partition(), id);

            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }


        public JsonResult rhoconnect_query_objects(String partition)
        {
            return Json(db.Contacts.ToDictionary(c => c.ID.ToString()));
        }

        public ActionResult rhoconnect_create(String objJson, String partition)
        {
            Contact new_contact = (Contact)RhoconnectNET.Helpers.deserialize_json(objJson, typeof(Contact));
            db.Contacts.Add(new_contact);
            db.SaveChanges();
            return RhoconnectNET.Helpers.serialize_result(new_contact.ID);
        }

        public ActionResult rhoconnect_update(Dictionary<string, object> changes, String partition)
        {
            int obj_id = Convert.ToInt32(changes["id"]);
            Contact contact_to_update = db.Contacts.First(c => c.ID == obj_id);
            // this method will update only the modified fields
            RhoconnectNET.Helpers.merge_changes(contact_to_update, changes);
            db.Entry(contact_to_update).State = EntityState.Modified;
            db.SaveChanges();
            return RhoconnectNET.Helpers.serialize_result(contact_to_update.ID);
        }

        public ActionResult rhoconnect_delete(Object objId, String partition)
        {
            int key = Convert.ToInt32(objId);

            Contact contact = db.Contacts.Find(key);
            db.Contacts.Remove(contact);
            db.SaveChanges();
            return RhoconnectNET.Helpers.serialize_result(key);
        }


    }
}