﻿using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using TomasosPizza.Models;

namespace TomasosPizza.Controllers
{
    public class OrderController : Controller
    {
        private TomasosContext _context;

        public OrderController(TomasosContext context)
        {
            _context = context;
        }

        public IActionResult AddToOrder(int id)
        {
            Matratt option = _context.Matratt.First(mat => mat.MatrattId == id);
            List<BestallningMatratt> order = Deserialize();
            BestallningMatratt hasOrdered = order.SingleOrDefault(p => p.MatrattId == option.MatrattId);
            if (hasOrdered!=null)
            {
                hasOrdered.Antal += 1;
            }
            else
            {
                hasOrdered = new BestallningMatratt
                {
                    Matratt = option,
                    MatrattId = option.MatrattId,
                    Antal = 1,
                };
                order.Add(hasOrdered);
            }
            
            Reserialize(order);

            return RedirectToAction("MenuView", "Navigation");
        }

        public IActionResult RemoveFromOrder(int id)
        {
            Matratt option = _context.Matratt.First(mat => mat.MatrattId == id);
            List<BestallningMatratt> order = Deserialize();
            BestallningMatratt remove = order.Find(o => o.MatrattId == option.MatrattId);
            if (remove.Antal<=1)
            {
                order.Remove(remove);
            }
            else
            {
                remove.Antal -= 1;
            }
            
            Reserialize(order);
            return RedirectToAction("MenuView", "Navigation");
        }

        private void Reserialize(List<BestallningMatratt> order)
        {
            var serializedValue = JsonConvert.SerializeObject(order);
            HttpContext.Session.SetString("Order", serializedValue);
        }
        private List<BestallningMatratt> Deserialize()
        {
            List<BestallningMatratt> order;
            if (HttpContext.Session.GetString("Order") == null)
            {
                order = new List<BestallningMatratt>();
            }
            else
            {
                var str = HttpContext.Session.GetString("Order");
                order = JsonConvert.DeserializeObject<List<BestallningMatratt>>(str);
            }

            return order;
        }

        public IActionResult CheckOut(Bestallning order)
        {
            // todo Add order to Beställning in DB, possibly link in BestallningMatratt
            _context.Bestallning.Add(order);
            foreach (var matratt in order.BestallningMatratt)
            {
                //_context.BestallningMatratt.Add()
            }
            return RedirectToAction("ThankYou","Navigation");
        }
    }
}
