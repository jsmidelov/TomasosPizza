﻿using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        public IActionResult CheckOut()
        {
            var serialized = HttpContext.Session.GetString("FinalOrder");
            Bestallning order = JsonConvert.DeserializeObject<Bestallning>(serialized);

            var b = new Bestallning
            {
                KundId = order.Kund.KundId,
                Levererad = true,
                Totalbelopp = order.Totalbelopp,
                BestallningDatum = order.BestallningDatum,
            };
            _context.Add(b);
            
            foreach (var matratt in order.BestallningMatratt)
            {
                var m = new BestallningMatratt
                {
                    BestallningId = b.BestallningId,
                    MatrattId = matratt.MatrattId,
                    // todo solve Antal to account for multiples
                    Antal = matratt.Antal
                };
                _context.Add(m);
            }
            _context.SaveChanges();
            return RedirectToAction("ThankYou","Navigation");
        }
    }
}
