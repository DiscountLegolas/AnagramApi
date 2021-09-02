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
    [Produces("application/json")]
    [Route("[controller]")]
    [ApiController]
    public class AnagramController : ControllerBase
    {
        /// <summary>
        /// Fetches Anagrams Of The word in turkish
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     Get /Anagram/papatya
        ///
        /// </remarks>
        /// <param name="kelime"></param>
        [Route("{kelime}")]
        [HttpGet]
        public IActionResult Anagramlar(string kelime)
        {
            HttpClient client = new HttpClient();
            List<ResourceMadde> kelimeler = new List<ResourceMadde>();
            var tümkelimeler = System.IO.File.ReadAllLines("TDK_Sözlük_Kelime_Listesi.txt");
            foreach (var item in tümkelimeler.Where(x=>CheckİfCharlistContainsAnotherCharlist(kelime,x)&&x.Length>1&&x.ToLower()!=kelime.ToLower()))
            {
                string uri = "https://sozluk.gov.tr/gts?ara=" + item;
                if (client.GetAsync(uri).Result.IsSuccessStatusCode)
                {
                    var asstringresult = client.GetAsync(uri).Result.Content.ReadAsStringAsync().Result;
                    var maddeler = JsonConvert.DeserializeObject<List<Madde>>(asstringresult);
                    foreach (var madde in maddeler)
                    {
                        kelimeler.Add(new ResourceMadde() { Kelime = madde.madde_duz, Anlamı = madde.anlamlarListe.First().anlam });
                    }
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
        [HttpGet]
        [Route("English/{word}")]
        public IActionResult Anagrams(string word)
        {
            HttpClient client = new HttpClient();
            List<ResourceMadde> words = new List<ResourceMadde>();
            var allwords= System.IO.File.ReadAllLines("words_alpha.txt");
            foreach (var item in allwords.Where(x => CheckİfCharlistContainsAnotherCharlist(word, x) && x.Length > 1 && x.ToLower() != word.ToLower()))
            {
                string uri = "https://api.dictionaryapi.dev/api/v2/entries/en/" + item;
                if (client.GetAsync(uri).Result.IsSuccessStatusCode)
                {
                    var asstringresult = client.GetAsync(uri).Result.Content.ReadAsStringAsync().Result;
                    var kelime = JsonConvert.DeserializeObject<List<Word>>(asstringresult).First();
                    var meaning = "";
                    foreach (var anlam in kelime.meanings)
                    {
                        if (kelime.meanings.Last()!=anlam)
                        {
                            meaning = meaning + anlam.definitions.First().definition + ",";
                        }
                        else
                        {
                            meaning = meaning + anlam.definitions.First().definition;
                        }
                    }
                    words.Add(new ResourceMadde() { Kelime = kelime.word, Anlamı = meaning });
                }
            }
            return Ok(words);
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
