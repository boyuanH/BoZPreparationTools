using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoZPreparation_Tool
{
    public class BozTask
    {
        public string exeFileName;
        public string exeParaStrs;

        public BozTask()
        {

        }

        public BozTask(string fileName,string parastrs)
        {
            exeFileName = fileName;
            exeParaStrs = parastrs;
        }

    }
}
