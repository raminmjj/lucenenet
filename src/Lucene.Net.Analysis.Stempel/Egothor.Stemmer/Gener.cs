﻿using Lucene.Net.Support;
using System.Collections.Generic;

/*
                    Egothor Software License version 1.00
                    Copyright (C) 1997-2004 Leo Galambos.
                 Copyright (C) 2002-2004 "Egothor developers"
                      on behalf of the Egothor Project.
                             All rights reserved.

   This  software  is  copyrighted  by  the "Egothor developers". If this
   license applies to a single file or document, the "Egothor developers"
   are the people or entities mentioned as copyright holders in that file
   or  document.  If  this  license  applies  to the Egothor project as a
   whole,  the  copyright holders are the people or entities mentioned in
   the  file CREDITS. This file can be found in the same location as this
   license in the distribution.

   Redistribution  and  use  in  source and binary forms, with or without
   modification, are permitted provided that the following conditions are
   met:
    1. Redistributions  of  source  code  must retain the above copyright
       notice, the list of contributors, this list of conditions, and the
       following disclaimer.
    2. Redistributions  in binary form must reproduce the above copyright
       notice, the list of contributors, this list of conditions, and the
       disclaimer  that  follows  these  conditions  in the documentation
       and/or other materials provided with the distribution.
    3. The name "Egothor" must not be used to endorse or promote products
       derived  from  this software without prior written permission. For
       written permission, please contact Leo.G@seznam.cz
    4. Products  derived  from this software may not be called "Egothor",
       nor  may  "Egothor"  appear  in  their name, without prior written
       permission from Leo.G@seznam.cz.

   In addition, we request that you include in the end-user documentation
   provided  with  the  redistribution  and/or  in the software itself an
   acknowledgement equivalent to the following:
   "This product includes software developed by the Egothor Project.
    http://egothor.sf.net/"

   THIS  SOFTWARE  IS  PROVIDED  ``AS  IS''  AND ANY EXPRESSED OR IMPLIED
   WARRANTIES,  INCLUDING,  BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
   MERCHANTABILITY  AND  FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
   IN  NO  EVENT  SHALL THE EGOTHOR PROJECT OR ITS CONTRIBUTORS BE LIABLE
   FOR   ANY   DIRECT,   INDIRECT,  INCIDENTAL,  SPECIAL,  EXEMPLARY,  OR
   CONSEQUENTIAL  DAMAGES  (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
   SUBSTITUTE  GOODS  OR  SERVICES;  LOSS  OF  USE,  DATA, OR PROFITS; OR
   BUSINESS  INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
   WHETHER  IN  CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE
   OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN
   IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

   This  software  consists  of  voluntary  contributions  made  by  many
   individuals  on  behalf  of  the  Egothor  Project  and was originally
   created by Leo Galambos (Leo.G@seznam.cz).
 */

namespace Egothor.Stemmer
{
    /// <summary>
    /// The Gener object helps in the discarding of nodes which break the reduction
    /// effort and defend the structure against large reductions.
    /// </summary>
    public class Gener : Reduce
    {
        /// <summary>
        /// Constructor for the Gener object.
        /// </summary>
        public Gener() { }

        /// <summary>
        /// Return a Trie with infrequent values occurring in the given Trie removed.
        /// </summary>
        /// <param name="orig">the Trie to optimize</param>
        /// <returns>a new optimized Trie</returns>
        public override Trie Optimize(Trie orig)
        {
            IList<string> cmds = orig.cmds;
            IList<Row> rows = new List<Row>();
            IList<Row> orows = orig.rows;
            int[] remap = new int[orows.Count];

            Arrays.Fill(remap, 1);
            for (int j = orows.Count - 1; j >= 0; j--)
            {
                if (Eat(orows[j], remap))
                {
                    remap[j] = 0;
                }
            }

            Arrays.Fill(remap, -1);
            rows = RemoveGaps(orig.root, orows, new List<Row>(), remap);

            return new Trie(orig.forward, remap[orig.root], cmds, rows);
        }

        /// <summary>
        /// Test whether the given Row of Cells in a Trie should be included in an
        /// optimized Trie.
        /// </summary>
        /// <param name="in">the Row to test</param>
        /// <param name="remap">Description of the Parameter</param>
        /// <returns><c>true</c> if the Row should remain; otherwise, <c>false</c></returns>
        public bool Eat(Row @in, int[] remap)
        {
            int sum = 0;
            foreach (Cell c in @in.cells.Values)
            {
                sum += c.cnt;
                if (c.@ref >= 0)
                {
                    if (remap[c.@ref] == 0)
                    {
                        c.@ref = -1;
                    }
                }
            }
            int frame = sum / 10;
            bool live = false;
            foreach (Cell c in @in.cells.Values)
            {
                if (c.cnt < frame && c.cmd >= 0)
                {
                    c.cnt = 0;
                    c.cmd = -1;
                }
                if (c.cmd >= 0 || c.@ref >= 0)
                {
                    live |= true;
                }
            }
            return !live;
        }
    }
}
