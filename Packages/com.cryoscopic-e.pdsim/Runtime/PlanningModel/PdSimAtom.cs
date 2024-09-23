using System;

namespace PDSim.PlanningModel
{
     /// <summary>
    /// Class representing an atom.
    /// e.g 3, true, kitchen, at-robot
    /// </summary>
    [Serializable]
    public class PdSimAtom
    {
        /// <summary>
        /// Type of value of an Atom
        /// </summary>
        [Serializable]
        public enum ValueType
        {
            Symbol,
            Int,
            Real,
            Boolean
        }


        public Atom.ContentOneofCase contentCase;

        public string valueSymbol;

        public PdSimAtom(Atom atom)
        {
            if (atom == null)
            {
                contentCase = Atom.ContentOneofCase.None;
                valueSymbol = string.Empty;
            }
            else
            {
                contentCase = atom.ContentCase;
                switch (contentCase)
                {
                    default:
                    case Atom.ContentOneofCase.Symbol:
                        valueSymbol = atom.Symbol;
                        break;
                    case Atom.ContentOneofCase.Int:
                        valueSymbol = atom.Int.ToString();
                        break;
                    case Atom.ContentOneofCase.Real:
                        valueSymbol = RealToFloat(atom.Real).ToString();
                        break;
                    case Atom.ContentOneofCase.Boolean:
                        valueSymbol = atom.Boolean.ToString();
                        break;
                }
            }
        }

        // Create a copy and new instance of the atom
        public PdSimAtom(PdSimAtom atom)
        {
            contentCase = atom.contentCase;
            valueSymbol = atom.valueSymbol;
        }

        private PdSimAtom()
        {
            contentCase = Atom.ContentOneofCase.None;
            valueSymbol = string.Empty;
        }

        private PdSimAtom(string value)
        {
            if (value == "up:not") // then is a boolean
            {
                contentCase = Atom.ContentOneofCase.Boolean;
                valueSymbol = bool.FalseString;
            }
            else if (value == "up:and")
            {
                contentCase = Atom.ContentOneofCase.Symbol;
                valueSymbol = "AND";
            }
            else
            {
                // TODO: support other function symbols
                contentCase = Atom.ContentOneofCase.Symbol;
                valueSymbol = value;
            }
        }

        private PdSimAtom(int value)
        {
            contentCase = Atom.ContentOneofCase.Int;
            valueSymbol = value.ToString();
        }

        private PdSimAtom(float value)
        {
            contentCase = Atom.ContentOneofCase.Real;
            valueSymbol = value.ToString();
        }

        private PdSimAtom(bool value)
        {
            contentCase = Atom.ContentOneofCase.Boolean;
            valueSymbol = value.ToString();
        }

        public bool IsEmpty()
        {
            return contentCase == Atom.ContentOneofCase.None;
        }

        public override string ToString()
        {
            return valueSymbol;
        }

        public static PdSimAtom Empty()
        {
            return new PdSimAtom(new Atom());
        }

        public static PdSimAtom Boolean(bool value)
        {
            return new PdSimAtom(value);
        }

        public static PdSimAtom Symbol(string value)
        {
            return new PdSimAtom(value);
        }

        public bool IsTrue()
        {
            return contentCase == Atom.ContentOneofCase.Boolean && bool.Parse(valueSymbol) == true;
        }

        public bool IncreaseValue(float amount)
        {
            if (contentCase != Atom.ContentOneofCase.Real)
                return false;

            var value = float.Parse(valueSymbol);
            value += amount;
            valueSymbol = value.ToString();
            return true;
        }

        public bool DecreaseValue(float amount)
        {
            if (contentCase != Atom.ContentOneofCase.Real)
                return false;

            var value = float.Parse(valueSymbol);
            value -= amount;
            valueSymbol = value.ToString();
            return true;
        }

        // Convert unified planning domain value type to PdSim ValueType
        public static ValueType ConvertValueType(string type)
        {
            if (type == "up:integer")
                return ValueType.Int;
            else if (type == "up:real")
                return ValueType.Real;
            else if (type == "up:bool")
                return ValueType.Boolean;
            else
                return ValueType.Symbol;
        }

        public static float RealToFloat(Real real)
        {
            return real.Numerator / real.Denominator;
        }
    }

    
}