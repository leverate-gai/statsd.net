﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace statsd.net.shared.Messages
{
  public sealed class Set : TaggedStatsdMessage
  {
    public string Value { get; set; }

    public Set(string name, string value, string[] tags = null) : base(tags)
    {
      Name = name;
      Value = value;
      MessageType = MessageType.Set;
    }

    public override string ToString()
    {
      return String.Format("{0}:{1}|s", Name, Value);
    }
  }
}
