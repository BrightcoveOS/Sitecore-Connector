using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brightcove.Core.EmbedGenerator.Models
{
    public class EmbedMarkup
    {
        public EmbedModel Model { get; set; } = new EmbedModel();

        public string Markup { get; set; } = "";
    }
}
