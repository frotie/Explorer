using Explorer.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Explorer
{
    internal class ViewModelLocator
    {
        public MainViewModel MainViewModel => new MainViewModel(); // DI
    }
}
