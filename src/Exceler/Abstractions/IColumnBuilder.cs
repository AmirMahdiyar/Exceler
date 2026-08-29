using Exceler.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exceler.Abstractions
{
    internal interface IColumnBuilder<TInput> where TInput : class, new()
    {
        void Compile(ExcelProfile<TInput> profile);
    }
}
