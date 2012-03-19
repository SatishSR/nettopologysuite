﻿using System;

namespace NetTopologySuite.Tests.NUnit.Performance
{
public class ExamplePerformanceTest : PerformanceTestCase
{
    private int _iter;

    public ExamplePerformanceTest(String name)
        :base(name)
  {
    RunSize = new[] {10, 20};
    RunIterations = 10;
  }

  public override void SetUp()
  {
    // read data and allocate resources here
  }
  
  public override void StartRun(int size)
  {
    Console.WriteLine("Running with size " + size);
    _iter = 0;
  }
  
  
  public void RunExample()
  {
    Console.WriteLine("Iter # " + _iter++);
    // do test work here
  }
  
  public override void TearDown()
  {
    // deallocate resources here
  }
}}