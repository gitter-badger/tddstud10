﻿module R4nd0mApps.TddStud10.Engine.Core.DataStoreTests

open Xunit
open R4nd0mApps.TddStud10.Common.Domain
open R4nd0mApps.TddStud10.Engine.TestFramework
open System
open System.Collections.Concurrent

let createDS slnPath = 
    let ds = DataStore() :> IDataStore
    RunStartParamsExtensions.create DateTime.Now (FilePath slnPath) |> ds.UpdateRunStartParams
    ds

let createDSWithPATC slnPath = 
    let ds = createDS slnPath
    let spy = CallSpy<PerDocumentLocationDTestCases>(Throws(Exception()))
    ds.TestCasesUpdated.Add(spy.Func >> ignore)
    ds, spy

let createDSWithPDSP slnPath = 
    let ds = createDS slnPath
    let spy = CallSpy<PerDocumentSequencePoints>(Throws(Exception()))
    ds.SequencePointsUpdated.Add(spy.Func >> ignore)
    ds, spy

let createDSWithTRO slnPath = 
    let ds = createDS slnPath
    let spy1 = CallSpy<PerTestIdDResults>(Throws(Exception()))
    ds.TestResultsUpdated.Add(spy1.Func >> ignore)
    let spy2 = CallSpy<PerDocumentLocationTestFailureInfo>(Throws(Exception()))
    ds.TestFailureInfoUpdated.Add(spy2.Func >> ignore)
    let spy3 = CallSpy<PerSequencePointIdTestRunId>(Throws(Exception()))
    ds.CoverageInfoUpdated.Add(spy3.Func >> ignore)
    ds, spy1, spy2, spy3

let createPATC (ts : (string * FilePath * FilePath * DocumentCoordinate) list) = 
    let patc = PerDocumentLocationDTestCases()
    
    let addTestCase (acc : PerDocumentLocationDTestCases) (f, s, d, l) = 
        let tc =  { FullyQualifiedName = f; DisplayName = ""; Source = s; CodeFilePath = d; LineNumber = l }
        let b = 
            acc.GetOrAdd({ document = d
                           line = l }, fun _ -> ConcurrentBag<_>())
        b.Add(tc) |> ignore
        acc
    ts |> Seq.fold addTestCase patc

let createPDSP() = PerDocumentSequencePoints()
let createTRO() = PerTestIdDResults(), PerDocumentLocationTestFailureInfo(), PerSequencePointIdTestRunId()

[<Fact>]
let ``UpdateData with PATV causes event to be fired and crash in handler is ignored``() = 
    let ds, spy = createDSWithPATC @"c:\a.sln"
    let patc = [] |> createPATC
    ds.UpdateData(patc |> TestCases)
    Assert.Equal(spy.CalledWith, Some patc)

[<Fact>]
let ``UpdateData with PDSP causes event to be fired and crash in handler is ignored``() = 
    let ds, spy = createDSWithPDSP @"c:\a.sln"
    let pdsp = () |> createPDSP
    ds.UpdateData(pdsp |> SequencePoints)
    Assert.Equal(spy.CalledWith, Some pdsp)

[<Fact>]
let ``UpdateData with TRO causes event to be fired and crash in handler is ignored``() = 
    let ds, spy1, spy2, spy3 = createDSWithTRO @"c:\a.sln"
    let ptir, pdtfi, paspc = () |> createTRO
    ds.UpdateData((ptir, pdtfi, paspc) |> TestRunOutput)
    Assert.Equal(spy1.CalledWith, Some ptir)
    Assert.Equal(spy2.CalledWith, Some pdtfi)
    Assert.Equal(spy3.CalledWith, Some paspc)
