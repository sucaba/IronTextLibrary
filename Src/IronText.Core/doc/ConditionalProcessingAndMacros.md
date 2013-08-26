        [SName("if")]
        public StxNode If(CalcExpr cond, StxNode pos, StxNode neg) 
        {
            return cond.Value == 0 ? neg : pos;
        }