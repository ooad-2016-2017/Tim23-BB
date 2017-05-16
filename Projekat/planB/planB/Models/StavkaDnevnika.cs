﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace planB.Models
{
    public class StavkaDnevnika : Stavka, INotifyPropertyChanged
    {
        String naslov;

        public event PropertyChangedEventHandler PropertyChanged;

        public StavkaDnevnika() : base() { }

        public StavkaDnevnika(DateTime _datum, String _sadrzaj, Vidljivost _vidljivost, String _naslov) :
            base(_datum, _sadrzaj, _vidljivost)
        {
            naslov = _naslov;
        }


        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        public String Naslov
        {
            get { return naslov; }
            set
            {
                naslov = value;
                NotifyPropertyChanged(nameof(Naslov));
            }
        }
    }
}
