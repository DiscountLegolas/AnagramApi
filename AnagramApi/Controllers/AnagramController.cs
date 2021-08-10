using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AnagramApi.Models;
using Newtonsoft.Json;
using System.IO;

namespace AnagramApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AnagramController : ControllerBase
    {
        [Route("{kelime}")]
        public IActionResult Anagramlar(string kelime)
        {
            HttpClient client = new HttpClient();
            List<ResourceMadde> kelimeler = new List<ResourceMadde>();
            var tümkelimeler = System.IO.File.ReadAllLines(@"C:\Users\prof1\OneDrive\Masaüstü\TDK_Sözlük_Kelime_Listesi.txt");
            foreach (var item in tümkelimeler.Where(x=>CheckİfCharlistContainsAnotherCharlist(kelime,x)&&x.Length>1&&x.ToLower()!=kelime.ToLower()))
            {
                string uri = "https://sozluk.gov.tr/gts?ara=" + item;
                var asstringresult = client.GetAsync(uri).Result.Content.ReadAsStringAsync().Result;
                var maddeler = JsonConvert.DeserializeObject<List<Madde>>(asstringresult);
                foreach (var madde in maddeler)
                {
                    kelimeler.Add(new ResourceMadde() { Kelime = madde.madde_duz, Anlamı =madde.anlamlarListe.First().anlam});
                }
            }
            for (int i = 0; i < kelimeler.Count; i++)
            {
                var item = kelimeler[i];
                if (kelimeler.Count(x => x.Kelime == item.Kelime && x.Anlamı == item.Anlamı) == 2)
                {
                    kelimeler.Remove(kelimeler.Last(x => x.Kelime == item.Kelime && x.Anlamı == item.Anlamı));
                }
            }
            return Ok(kelimeler);
        }
        private bool CheckİfCharlistContainsAnotherCharlist(string  word,string  word2)
        {
            var charlist1 = word.ToLower().ToList();
            var charlist2 = word2.ToLower().ToList();
            bool yes = true;
            foreach (var item in charlist2)
            {
                if (!charlist1.Any(x=>x==item))
                {
                    yes = false;
                }
                else
                {
                    charlist1.Remove(charlist1.First(x => x == item));
                }
            }
            return yes;
        }
    }
}
