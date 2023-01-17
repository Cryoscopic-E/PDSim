using System;
using Unity.VisualScripting;

namespace PDSim.Types
{
    [Serializable, Inspectable]
    public class PDBooleanFluent : PDFluent
    {
        [Inspectable]
        public bool value;
    }
}