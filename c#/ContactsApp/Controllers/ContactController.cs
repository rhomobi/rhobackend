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

        //rhoconnect查询方法
        public JsonResult Query() {
            List<Contact> list = db.Contacts.ToList<Contact>();
            List<Dictionary<String, Contact>> newlist = new List<Dictionary<String, Contact>>();
            foreach (Contact model in list)
            {
                Dictionary<String, Contact> dic = new Dictionary<string, Contact>();
                dic.Add("contact", model);
                newlist.Add(dic);
            }

            return Json(newlist, JsonRequestBehavior.AllowGet);

            //return rhoconnect_query_objects(partition());
        }

        //
        // GET: /Contact/Details/5

        public ViewResult Details(int id)
        {
            Contact contact = db.Contacts.Find(id);
            return View(contact);
        }



        // rhoconnect插入后查询方法
        // GET: /Contact/Detail/5
        public JsonResult Detail(String id)
        {
            Contact contact = db.Contacts.Find(Convert.ToInt32(id.ToLower().Replace(".json","")));
            Dictionary<String, Contact> dic = new Dictionary<string, Contact>();
            dic.Add("contact", contact);
            return Json(dic, JsonRequestBehavior.AllowGet);
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
			return "app";
            //return "testuser";
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
                //RhoconnectNET.Client.notify_on_create(partition(), contact);

                return RedirectToAction("Index");
            }

            return View(contact);
        }

        /// <summary>
        /// rhoconnect插入方法
        /// </summary>
        /// <param name="contact"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Insert(Contact contact)
        {
            if (ModelState.IsValid)
            {
                contact = new Contact();
                contact.FirstName = Request.Params["contact[FirstName]"];
                contact.LastName = Request.Params["contact[LastName]"];
                contact.Phone = Request.Params["contact[Phone]"];
                contact.Email = Request.Params["contact[Email]"];

                return rhoconnect_create("{\"FirstName\":\"" + Request.Params["contact[FirstName]"] + "\",\"LastName\":\"" + Request.Params["contact[LastName]"] + "\",\"Phone\":\"" + Request.Params["contact[Phone]"] + "\",\"Email\":\"" + Request.Params["contact[Email]"] + "\"}", partition());
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
                //RhoconnectNET.Client.notify_on_update(partition(), contact);

                return RedirectToAction("Index");
            }
            return View(contact);
        }

        /// <summary>
        /// rhoconnect更新方法
        /// </summary>
        /// <param name="contact"></param>
        /// <returns></returns>
        [HttpPut]
        public ActionResult Update(int id,Contact contact)
        {
            if (ModelState.IsValid)
            {
                Dictionary<string, object> changes=new Dictionary<string,object>();
                changes.Add("id",id);
                if (Request.Params["contact[FirstName]"] != null)
                {
                    changes.Add("FirstName",Request.Params["contact[FirstName]"]);
                }
                if (Request.Params["contact[LastName]"] != null)
                {
                    changes.Add("LastName",Request.Params["contact[LastName]"]);
                }
                if (Request.Params["contact[Phone]"] != null)
                {
                    changes.Add("Phone",Request.Params["contact[Phone]"]);
                }
                if (Request.Params["contact[Email]"] != null)
                {
                    changes.Add("Email",Request.Params["contact[Email]"]);
                }

                return rhoconnect_update(changes, partition());
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
            //bool rs=RhoconnectNET.Client.notify_on_delete("Contact", partition(), id);

            return RedirectToAction("Index");
        }

        /// <summary>
        /// rhoconnect删除方法
        /// </summary>
        /// <param name="contact"></param>
        /// <returns></returns>
        [HttpDelete]
        public ActionResult DropById(int id)
        {
            //Contact contact = db.Contacts.Find(id);
            //db.Contacts.Remove(contact);
            //db.SaveChanges();
            //RhoconnectNET.Client.notify_on_delete("Contact", partition(), id);

            //return RedirectToAction("Index");

            return rhoconnect_delete(id, partition());
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }


        public JsonResult rhoconnect_query_objects(String partition)
        {
            return Json(db.Contacts.ToDictionary(c => c.ID.ToString()),JsonRequestBehavior.AllowGet);
        }

        public ActionResult rhoconnect_create(String objJson, String partition)
        {
            Contact new_contact = (Contact)RhoconnectNET.Helpers.deserialize_json(objJson, typeof(Contact));
            db.Contacts.Add(new_contact);
            db.SaveChanges();
            String headerstr = "http://localhost:8081/contact/detail/" + new_contact.ID.ToString();
            Response.AppendHeader("Location", headerstr);
            return RhoconnectNET.Helpers.serialize_result(new_contact.ID);
        }

        public ActionResult rhoconnect_update(Dictionary<string, object> changes, String partition)
        {
            int obj_id = Convert.ToInt32(changes["id"]);
            Contact contact_to_update = db.Contacts.First(c => c.ID == obj_id);
            // this method will update only the modified fields
            //RhoconnectNET.Helpers.merge_changes(contact_to_update, changes);
            db.Entry(contact_to_update).State = EntityState.Modified;
            if (changes.ContainsKey("FirstName") == true)
            {
                contact_to_update.FirstName = changes["FirstName"].ToString();
            }
            if (changes.ContainsKey("LastName")==true)
            {
                contact_to_update.LastName = changes["LastName"].ToString();
            }
            if (changes.ContainsKey("Phone")==true)
            {
                contact_to_update.Phone = changes["Phone"].ToString();
            }
            if (changes.ContainsKey("Email")==true)
            {
                contact_to_update.Email = changes["Email"].ToString();
            }
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