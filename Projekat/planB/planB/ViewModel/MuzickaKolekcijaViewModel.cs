﻿using Microsoft.WindowsAzure.MobileServices;
using planB.AzureModels;
using planB.Models;
using planB.Services;
using planB.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace planB.ViewModel
{
    public class MuzickaKolekcijaViewModel : INotifyPropertyChanged
    {
        public MessageDialog Poruka { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;


        private String searchingText;
        public ObservableCollection<Pjesma> rezultatPretrage { get; set; }
        private PretragaMuzike pretragaMuzike;
        private LastFmPretraga lastFmPretraga;

        public Korisnik TrenutniKorisnik { get; set; }

        public ObservableCollection<MuzickaKolekcija> Kolekcija { get; set; }
        public MuzickaKolekcija odabranaKolekcija { get; set; }
        public ObservableCollection<Pjesma> pjesmeIzOdabraneKolekcije { get; set; }

        private bool searchingEnabled;

        private Pjesma _odabranaPjesma;

        public MuzickaKolekcijaViewModel()
        {
            SearchingText = "";
            TrenutniKorisnik = LoginViewModel.korisnik;
            pretragaMuzike = new PretragaMuzike();
            lastFmPretraga = new LastFmPretraga();
            rezultatPretrage = new ObservableCollection<Pjesma>();
            SearchingEnabled = true;

            if (TrenutniKorisnik.MuzickaKolekcija.Count == 0)
            {
                dajKolekcije();
            }

            Kolekcija = new ObservableCollection<MuzickaKolekcija>(TrenutniKorisnik.MuzickaKolekcija);
        }



        public MuzickaKolekcijaViewModel(Korisnik trenutniKorisnik)
        {
            TrenutniKorisnik = trenutniKorisnik;
            SearchingEnabled = false;
            if (TrenutniKorisnik.MuzickaKolekcija.Count == 0)
            {
                dajKolekcije();
            }
            Kolekcija = new ObservableCollection<MuzickaKolekcija>(TrenutniKorisnik.MuzickaKolekcija);
        }

        public ObservableCollection<Pjesma> RezultatiPretrage
        {
            get { return rezultatPretrage; }
            set
            {
                rezultatPretrage = value;
                NotifyPropertyChanged(nameof(RezultatiPretrage));
            }
        }

        public String SearchingText
        {
            get { return searchingText; }
            set
            {
                searchingText = value;
                NotifyPropertyChanged(nameof(SearchingText));
            }
        }

        public MuzickaKolekcija OdabranaKolekcija
        {
            get { return odabranaKolekcija; }
            set
            {
                odabranaKolekcija = value;
                PjesmeIzOdabraneKolekcije = new ObservableCollection<Pjesma>(odabranaKolekcija.Pjesme);
            }
        }

        public ObservableCollection<Pjesma> PjesmeIzOdabraneKolekcije
        {
            get { return pjesmeIzOdabraneKolekcije; }
            set
            {
                pjesmeIzOdabraneKolekcije = value;
                NotifyPropertyChanged(nameof(PjesmeIzOdabraneKolekcije));
            }
        }

        public Pjesma _OdabranaPjesma
        {
            get { return _odabranaPjesma; }
            set
            {
                _odabranaPjesma = value;
                ProfilPage.frame.Navigate(typeof(PrikazPjesmeForm), new PjesmaViewModel(_odabranaPjesma, TrenutniKorisnik));
            }
        }

        public bool SearchingEnabled
        {
            get { return searchingEnabled; }
            set
            {
                searchingEnabled = value;
                NotifyPropertyChanged(nameof(SearchingEnabled));
            }
        }

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        public async void Search_Artist(String search)
        {
            SearchingText = search;
            rezultatPretrage = new ObservableCollection<Pjesma>();
            SearchingResult rezultatWEBPretrage = new SearchingResult();
            try
            {
                rezultatWEBPretrage = await lastFmPretraga.Search_Artists(SearchingText);
            }
            catch (Exception e)
            {
                //Poruka = new MessageDialog(e.ToString());
                //await Poruka.ShowAsync();
            }
            finally
            {

            }

            foreach (Track track in rezultatWEBPretrage.Tracks)
            {
                Pjesma pjesma = new Pjesma();
                pjesma.Izvodjac = rezultatWEBPretrage.Artist;
                pjesma.Naziv = track.Name;
                pjesma.Preview = track.Preview;
                pjesma.UrlSlike = track.PhotoUrl;
                rezultatPretrage.Add(pjesma);
            }
        }

        public void PrikaziPjesmu(Pjesma odabranaPjesma)
        {
            ProfilPage.frame.Navigate(typeof(PrikazPjesmeForm), new PjesmaViewModel(odabranaPjesma, TrenutniKorisnik, searchingEnabled));
        }

        public async void dodajNovuKolekciju(String NazivNoveKolekcije, Pjesma odabranaPjesma)
        {
            using (var DB = new PlanBDbContext())
            {
                MuzickaKolekcija postojecaKolekcija = DB.MuzickaKolekcija.Where(x => (x.Naziv == NazivNoveKolekcije)).FirstOrDefault();
                if (postojecaKolekcija != null)
                {
                    Poruka = new MessageDialog("Postoji kolekcija sa unesenim imenom.");
                    await Poruka.ShowAsync();
                    return;
                }
                if (NazivNoveKolekcije.Length < 4)
                {
                    Poruka = new MessageDialog("Naziv mora imati bar 4 znaka.");
                    await Poruka.ShowAsync();
                    return;
                }

                Pjesma postojecaPjesma = null; // DB.Pjesme.Where(x => (x.kolekcijaAzure == odabranaPjesma.ID)).FirstOrDefault();
                if (postojecaPjesma != null)
                {
                    Poruka = new MessageDialog("Pjesma vec postoji u kolekciji.");
                    await Poruka.ShowAsync();
                    return;
                }

                IMobileServiceTable<KolekcijaAzure> userTableObj = App.MobileService.GetTable<KolekcijaAzure>();
                KolekcijaAzure novaKolekcija1 = new KolekcijaAzure();
                novaKolekcija1.naziv = NazivNoveKolekcije;
                novaKolekcija1.datumKreiranja = DateTime.Today;
                novaKolekcija1.korisnikID = TrenutniKorisnik.idAzure;
                List<KolekcijaAzure> listaAzure = await userTableObj.Where(x => x.id != "").ToListAsync();
                novaKolekcija1.redniBroj = listaAzure.Count + 1;
                await userTableObj.InsertAsync(novaKolekcija1);

                odabranaPjesma.kolekcijaAzure = novaKolekcija1.id;

                MuzickaKolekcija novaKolekcija = new MuzickaKolekcija();
                novaKolekcija.Naziv = NazivNoveKolekcije;
                novaKolekcija.idAzure = TrenutniKorisnik.idAzure;
                
                // M A I D DODAO
                novaKolekcija.Pjesme.Add(odabranaPjesma);
                novaKolekcija.DatumKreiranja = DateTime.Today;
                TrenutniKorisnik.MuzickaKolekcija.Add(novaKolekcija);
                novaKolekcija.KorisnikID = TrenutniKorisnik.ID;
                DB.Pjesme.Add(odabranaPjesma);
                DB.MuzickaKolekcija.Add(novaKolekcija);
                DB.SaveChanges();
                Kolekcija.Add(novaKolekcija);
                Poruka = new MessageDialog("Pjesma uspješno dodana u kolekciju " + NazivNoveKolekcije + ".");
                await Poruka.ShowAsync();
            }
        }

        public async void dodajUPostojecu(MuzickaKolekcija odabranaKolekcija, Pjesma OdabranaPjesma)
        {
            using (var DB = new PlanBDbContext())
            {
                MuzickaKolekcija muzickaKolekcija = DB.MuzickaKolekcija.Where(x => (x.Naziv == odabranaKolekcija.Naziv && x.KorisnikID == TrenutniKorisnik.ID)).FirstOrDefault();

                foreach (Pjesma p in DB.Pjesme)
                {
                    MuzickaKolekcija muzcikaKolekcija = DB.MuzickaKolekcija.Where(x => (x.idAzure == p.kolekcijaAzure)).FirstOrDefault();
                    if (p.Naziv == OdabranaPjesma.Naziv && odabranaKolekcija.ID == muzcikaKolekcija.ID)
                    {
                        Poruka = new MessageDialog("Pjesma vec postoji u kolekciji.");
                        await Poruka.ShowAsync();
                        return;
                    }
                }

                Pjesma novaPjesma = new Pjesma(OdabranaPjesma.Izvodjac, OdabranaPjesma.Naziv, OdabranaPjesma.Preview, OdabranaPjesma.UrlSlike);
                novaPjesma.kolekcijaAzure = odabranaKolekcija.idAzure;


                IMobileServiceTable<PjesmaAzure> userTableObj = App.MobileService.GetTable<PjesmaAzure>();
                PjesmaAzure pjesmaAzure = new PjesmaAzure();
                pjesmaAzure.naziv = novaPjesma.Naziv;
                pjesmaAzure.preview = novaPjesma.Preview;
                pjesmaAzure.urlSlike = novaPjesma.UrlSlike;
                pjesmaAzure.izvodjac = novaPjesma.Izvodjac;
                pjesmaAzure.muzickaKolekcijaID = novaPjesma.kolekcijaAzure;
                await userTableObj.InsertAsync(pjesmaAzure);
                List<PjesmaAzure> listaAzure = await userTableObj.Where(x => x.id != "").ToListAsync();
                pjesmaAzure.redniBroj = listaAzure.Count + 1;
                muzickaKolekcija.Pjesme.Add(novaPjesma);
                TrenutniKorisnik.MuzickaKolekcija.Where(x => (x.ID == muzickaKolekcija.ID)).FirstOrDefault().Pjesme.Add(novaPjesma);

                DB.Pjesme.Add(novaPjesma);
                DB.SaveChanges();
                Poruka = new MessageDialog("Pjesma uspješno dodana u kolekciju " + odabranaKolekcija.Naziv + ".");
                await Poruka.ShowAsync();
            }
        }

        public async void dajKolekcije()
        {
            try
            {
                using (var DB = new PlanBDbContext())
                {
                    foreach (MuzickaKolekcija mk in DB.MuzickaKolekcija)
                    {
                        if (mk.Pjesme.Count == 0) { }
                        if (mk.KorisnikID == TrenutniKorisnik.ID)
                        {
                            foreach (Pjesma p in DB.Pjesme)
                            {
                                if (p.kolekcijaAzure == mk.idAzure && !mk.Pjesme.Contains(p))
                                    mk.Pjesme.Add(p);
                            }

                            TrenutniKorisnik.MuzickaKolekcija.Add(mk);
                        }
                    }
                }
                Kolekcija = new ObservableCollection<MuzickaKolekcija>(TrenutniKorisnik.MuzickaKolekcija);
            }
            catch (Exception e)
            {
                Poruka = new MessageDialog(e.Message);
                await Poruka.ShowAsync();
            }
        }
    }
}