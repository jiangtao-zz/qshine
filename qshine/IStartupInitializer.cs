﻿using System;
using System.Collections.Generic;
using System.Text;

namespace qshine
{
    /// <summary>
    /// Indicate a static constructor has been implemented.
    /// This static constructor will be called no later ApplicationEnvironment is built.
    /// The purpose of this interface is to initialize a specific type of class when the application environment begin.
    /// 
    /// The typical usage of the interface is to hook ApplicationEnvironment interceptor events
    /// to process activities before and after ApplicationEnvironment built.
    /// 
    /// Ex:
    ///     public class SampleC:IStartupInitializer
    ///     {
    ///         static SampleC()
    ///         {
    ///             var interceptor = Interceptor.Get&lt;ApplicationEnvironment&gt;();
    ///             interceptor.OnEnter += __applicationInit; //before load application environment setting
    ///             interceptor.OnSuccess += _applicationStartup; //after application environment setting loaded
    ///         }
    ///         static void __applicationInit(object sender, InterceptorEventArgs args)
    ///         {
    ///             if(args.MethodName=="Init"){
    ///                 ApplicationEnvironment app = (ApplicationEnvironment)sender;
    ///                 string environmentName = args.Args[0];
    ///                 EnvironmentInitializationOption environmentName = args.Args[1];
    ///                 ...
    ///             }
    ///         }
    ///         static void __applicationStartup(object sender, InterceptorEventArgs args)
    ///         {
    ///             if(args.MethodName=="Init"){
    ///                 ApplicationEnvironment app = (ApplicationEnvironment)sender;
    ///                 string environmentName = args.Args[0];
    ///                 EnvironmentInitializationOption environmentName = args.Args[1];
    ///                 ...
    ///             }
    ///         }
    ///     }
    /// </summary>
    public interface IStartupInitializer
    {

    }
}