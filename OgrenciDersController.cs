using DersDagitim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GrafRenklendirme.Controllers
{
    public class OgrenciDersController : Controller
    {
        // GET: OgrenciDers

        DersDagitimDBEntities db = new DersDagitimDBEntities();
        string[] renkler = { "white", "red", "purple", "yellow", "pink", "blue", "chocolate", "wheat", "green", "brown", "orange", "gray" };
        Dictionary<Ders, int> dersRenk = new Dictionary<Ders, int>();
        int kullanilanRenkSayisi = 0;
        const int gunlukDersSayisi = 8;
        const int gunSayisi = 7;

        #region CRUD
  	// OgrenciDers içi crud işlemleri
        #endregion

        public ActionResult Yap()
        {
            // [1] ders komşulukları çıkar = o dersi alan öğrencilerin diğer dersleri
            // [2] dersin derecesi = dersin komşu sayısı
            // [3] dersleri derecelerine göre büyükten küçüğe sırala 
            // [4] ilk rengi ilk derse ver 
            // [5] renk verilmemiş ilk dersi bul
            // [6] musait rengi ver -> goto 5
            return View();
        }

        public List<Ders> KomsulariniVer(Ders ders)
        {
            List<Ders> sonuc = new List<Ders>();

            // o dersin tüm öğrencileri
            var dersinOgrencileri = db.OgrenciDers.Where(x => x.Ders_Id == ders.Id).Select(a => a.Ogrenci_Id);

            // bu öğrencilerin aldığı diğer dersleri(komşuları) tespit eder
            foreach (var item in dersinOgrencileri)
                foreach (var item2 in db.OgrenciDers.Where(x => x.Ogrenci_Id == item).ToList())
                    if (item2.Ders_Id != ders.Id)
                        if (!sonuc.Any(x => x.Id == item2.Ders_Id))             // ders listeye eklenmemişse
                            sonuc.Add( db.Ders.First(x=>x.Id == item2.Ders_Id));// ekle

            return sonuc;
        }

        public List<Ders> KomsuOlmayanlariVer(Ders ders)
        {
            List<Ders> liste = db.Ders.ToList(); // tüm dersler
            List<Ders> sonuc = new List<Ders>(); // sadece komşu olmayanları tutacak liste

            foreach (var item in liste)
                if (!KomsulariniVer(item).Any(x => x.Id == ders.Id) && item.Id != ders.Id) // kendisi ve komşusu değilse
                    sonuc.Add(item);

            return sonuc;
        }

        public int DerecesiniVer(Ders ders)
        {
            // derece = komşusu sayısı 
            return KomsulariniVer(ders).Count;
        }

        public ActionResult Komsuluk()
        {
            List<Ders> dersler = db.Ders.ToList();
            int[,] komsuluk = new int[dersler.Count, dersler.Count];

            // tabloyu temizle
            for (int i = 0; i < dersler.Count; i++)
                for (int j = 0; j < dersler.Count; j++)
                {
                    komsuluk[i, j] = 0; // 0 -> ilişki yok
                }

            // tabloyu doldur (Adjacency matrix)
            for (int i = 0; i < dersler.Count; i++)
            {
                List<Ders> liste = KomsulariniVer(dersler[i]);
                for (int j = 0; j < liste.Count; j++)
                {
                    komsuluk[i, dersler.IndexOf(liste[j])] = 1; // 1 -> ilişki var
                }
            }

            ViewBag.Komsuluk = komsuluk;
            return PartialView(dersler);
        }

        public ActionResult Derece()
        {
            List<Ders> dersler = db.Ders.ToList();
            Dictionary<Ders, int> dereceler = new Dictionary<Ders, int>();

            // derece listesini oluştur
            foreach (var item in dersler)
                dereceler.Add(item, DerecesiniVer(item));

            // büyükten küçüğe sıralanmış dereceler
            dereceler = dereceler.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);

            return PartialView(dereceler);
        }

        public int MusaitRengiVer(Ders ders)
        {
            // komşularının renklerinin indexlerini listele
            List<int> komsularinRenkIndexleri = new List<int>();
            foreach (var item in KomsulariniVer(ders))
            {
                int index = dersRenk.FirstOrDefault(x => x.Key == item).Value;
                if (index > 0) // 0 -> beyaz renk(default)
                    komsularinRenkIndexleri.Add(index);
            }

            // komşularından farklı ilk rengi gönder
            for (int i = 1; i < renkler.Length; i++)
                if (!komsularinRenkIndexleri.Any(x => x == i))
                {
                    // kullanılan renk sayısını güncelle
                    if (i > kullanilanRenkSayisi)
                        kullanilanRenkSayisi = i;
                    return i;
                }

            return 1; // ilk rengin indexi
        }

        public ActionResult Renk()
        {
            List<Ders> dersler = db.Ders.ToList();
            Dictionary<Ders, int> dereceler = new Dictionary<Ders, int>();

            // derece listesini oluştur
            foreach (var item in dersler)
                dereceler.Add(item, DerecesiniVer(item));

            // büyükten küçüğe sıralanmış dereceler
            dereceler = dereceler.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);

            // ilk rengi ilk derse ver
            dersRenk.Add(dereceler.FirstOrDefault().Key, 1);

            // önce tüm dersleri, renkleri 0(beyaz) olacak şekilde ekle
            foreach (var item in dereceler)
                if (!dersRenk.Any(x => x.Key == item.Key))
                    dersRenk.Add(item.Key, 0);

            // renkleri ver
            foreach (var item in dereceler)
                if (dersRenk.FirstOrDefault(x => x.Key == item.Key).Value == 0) // henüz o derse renk verilmediyse
                    dersRenk[item.Key] = MusaitRengiVer(item.Key);              // musait rengi ver

            ViewBag.kullanilanRenkSayisi = kullanilanRenkSayisi;
            ViewBag.Renkler = renkler;
            return PartialView(dersRenk);
        }

    }
}