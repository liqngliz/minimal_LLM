﻿using Autofac;
using Configuration;
using Context;
using IoC;
using LLama;
using LLama.Abstractions;
using Factory;
using Reasoners;
using Llm;

namespace LlmFactoryTest;
[Collection("Sequential")]
public class LlmReasonerFactoryTest
{
    readonly IModule<Config> _IoC;
    private IFactory<IReasoner<Reasoning, ReasonerTemplate>> _sut;
  
    public LlmReasonerFactoryTest()
    {
        _IoC = new IoCModule("config.json");
    }

    [Theory]
    [InlineData(typeof(LlmReasoner))]
     public void Should_resolve_reasoner(Type expected)
    {   var llm = _IoC.Container().Resolve<Illm<IAsyncEnumerable<string>, string, LlmContextInstance,bool>>();
        _sut = new ReasonerFactory(llm);

        var actual = _sut.Make(expected);
        Assert.IsType(expected, actual);
    }
}
