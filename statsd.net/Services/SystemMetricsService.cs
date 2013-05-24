﻿using statsd.net.Messages;
using statsd.net.Framework;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using statsd.net.Listeners;
using statsd.net.Backends;

namespace statsd.net.Services
{
  public interface ISystemMetricsService
  {
    void Log(string name, int quantity = 1);
    void SetTarget(ITargetBlock<GraphiteLine> target);
  }

  /// <summary>
  /// Keeps track of things like bad lines, failed sends, lines processed etc.
  /// </summary>
  public class SystemMetricsService : ISystemMetricsService
  {
    private string _prefix;
    private ITargetBlock<GraphiteLine> _target;
    private ConcurrentDictionary<string, int> _metrics;

    public SystemMetricsService(string prefix = null, IIntervalService intervalService = null)
    {
      if (intervalService == null)
      {
        intervalService = new IntervalService(30);
      }
      _prefix = (prefix + ".") ?? String.Empty;
      _metrics = new ConcurrentDictionary<string, int>();
      intervalService.Elapsed += SendMetrics;
    }

    public void Log(string name, int quantity = 1)
    {
      _metrics.AddOrUpdate(name, quantity, (key, input) => { return input + quantity; });
    }

    public void SetTarget(ITargetBlock<GraphiteLine> target)
    {
      _target = target;
    }

    private void SendMetrics(object sender, IntervalFiredEventArgs args)
    {
      if (_target == null)
      {
        return;
      }

      // Get a count of metrics waiting to be sent out
      var outputBufferCount = SuperCheapIOC.ResolveAll<IBackend>().Sum(p => p.OutputCount);
      _target.Post(new GraphiteLine("outputBuffer", outputBufferCount));

      var pairs = _metrics.ToArray();
      _metrics.Clear();
      foreach (var pair in pairs)
      {
        _target.Post(new GraphiteLine(pair.Key, pair.Value));
      }
    }
  }
}