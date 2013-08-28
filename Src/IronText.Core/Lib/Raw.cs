#if SWITCH_FEATURE
using System;
using System.Collections.Generic;
using IronText.Framework;
using IronText.Lib.Ctem;
using IronText.Framework;

namespace IronText.Lib
{
    /// <summary>
    /// Raw is unparsed sequence of tokens.
    /// It can be either any non-delimiter token or 
    /// sequence of Raw tokens surrounded by the Opn Cls tokens.
    /// </summary>
	public class Raw : List<Msg>, IReceiver<Msg>
    {
        private int      nestLevel = 0;
        private bool     done;
        private readonly IReceiver<Msg> exit;
        private readonly int thisId;
        private readonly int OpnId;
        private readonly int ClsId;

        public Raw(IReceiver<Msg> exit, ILanguage language) 
        { 
            this.exit = exit;
            this.thisId = language.Identify(typeof(Raw));
            this.OpnId = language.Identify(StemScanner.LParen);
            this.ClsId = language.Identify(StemScanner.RParen);
        }

        IReceiver<Msg> IReceiver<Msg>.Next(Msg msg)
        {
            var id = msg.Id;

            if (done)
            {
                throw new InvalidOperationException();
            }

            if (id == OpnId)
            {
                ++nestLevel;
            }
            else if (id == ClsId)
            {
                --nestLevel;
            }

            if (nestLevel == 0)
            {
                done = true;
            }

            this.Add(msg);

            return done ? exit.Next(new Msg { Id = thisId, Value = this }) : this;
        }

        IReceiver<Msg> IReceiver<Msg>.Done()
        {
            throw new NotImplementedException("EOF is not yet supported in Raw class");
        }
    }
}

#endif
