# Intelligent Configuration and Operations Monitoring Dashboard (iCOM)

## Rationale behind iCOM

Part of their production support mandate, the Operations teams have to not only understand the nature of the system failures or degradation in functionality and performance, but also foresee the troubles laying ahead based on the current parameters.
Machine Learning technology can help with that, by applying predictive models to the huge amounts of textual information stored in various log files attached to various applications, components and tools.
In order to achieve that we can either use various models, either linear regression based (and hence applicable to time series based events) or categorical (with applicability to event classification.)
The proposed solution, title Intelligent Configuration and Operations Monitoring (iCOM) is using a few popular classification algorithms, powering models with supervised training, to achieve a few goals:
* automatically categorise the events criticality level
* predict the areas that are prone to increased degradation of functionality and performance
* render information about the usage level of various components of the system
* etc.   

## Component Design

The design is based on a rich client/action & data service pattern, with a combination of user and roles authenticated through Azure OpenID Connect, and permissions managed through a local identity management component. "Permissions" are seen here as the highest granularity level of authroization control, applicable to the method/function level.The local permissions are to be seen as an extension to Azure AD roles. As an alternative the roles "scopes" can be used as permissions store.

The rich client is using a Single Page Application (SPA) implementation that enhances the user experience and decreases the traffic between the browser and the backend service.

The design makes use of **Command-Query-Separation pattern (CQRS)** where the data model powering the services middle-tier is divided into **commands** (i.e. change the state of a system but do not return a value) and **queries** (i.e. return a result and do not change the observable state of the system) depending on the directin of data (writing vs reading) 

Below is the component diagram view.</br>

![iCOM Dashboard Design](https://user-images.githubusercontent.com/6631390/88387494-8b9c3680-cd80-11ea-9fec-29d169cd8200.png)

More reading at:</br></br>
https://martinfowler.com/bliki/CQRS.html</br>
https://martinfowler.com/bliki/CommandQuerySeparation.html


## Implementation Option: Blazor with Web Assembly

* Web Assembly compiles code into low-level byte code Web Assembly Modules (WASM) that Web browsers can directly execute without having to parse a source file.
* Web Assembly is a parallel technology to JavaScript and exists side-by-side with the JavaScript runtime in the **web browser virtual machine (VM)*.
* Web Assembly runs in the same browser sandbox that JavaScript uses and so has many of the same limitations. The security context is the same, and Web Assembly cannot access the underlying hardware or operating system outside of the sandbox. 
* Web Assembly is not a plug-in like Silverlight was, for example.
* Web Assembly is providing the potential for different languages and development models in the browser.

**Browser VM is built in JVM as a way to sandbox content. A web browser downloads two types to content , static content and a more advanced dynamic content; now the latter content needs to be sand boxed as it can sometimes be harmful to the client machine*

## Browsers Runtime Components
There are a number of features WebAssembly does not support, therefore Blazor does not natively support them natively. To access these browser features we need to use JavaScript as an intermediary between Blazor and the Browser by using JSInterop libs

![](https://user-images.githubusercontent.com/6631390/86378511-be5a7f80-bc57-11ea-8645-a9015cc6f605.png)

## Web Assembly and Browser Performance

* Web Assembly Modules (WASM) contain lower-level assembly-language-like intermediate code that can be produced by compilers of other languages. 
* WASM code doesn't need to be parsed like JavaScript because it’s already byte code that’s resolved into execution-ready byte code. 
* WASM is a binary format but can also be expressed in text format. It deals with instructions at the register, stack, and memory level. 
* WASM creates platform-agnostic byte code that is then compiled into native code for the appropriate computer platform (x86 or ARM) and executed on the specific browser platform. 
* Byte-code can be advantageous for creating very high-performance computational code, which can be highly optimized for performance and can execute considerably faster than JavaScript code both in terms of initial load time and runtime execution.

## .NET Implementation of WASM (a.k.a. Blazor Web Assembly)

* Microsoft's Blazor framework uses Web Assembly to bootstrap higher-level runtimes that can then execute higher-level languages like .NET code. 
* Blazor uses a Mono-compiled version of the .NET Runtime compiled to a WASM module to execute **.NET Standard 2.0 modules*
* Blazor creates a Mono.wasm, a browser-customized version of the Mono .NET Runtime compiled to Web Assembly that allows for bootstrapping .NET Standard assemblies and executing.NET code.
* Rather than compiling every bit of .NET code that your application runs to WASM, only the Mono Runtime is compiled as a WASM module. All of the Blazor framework and the application code is loaded as plain old .NET assemblies that are executed through Mono. 
* It allows regular .NET development, such as importing and referencing additional .NET Standard assemblies and instantiating classes and executing code in them.

**NET Standard 2.0 replaces Portable Class Libraries (PCLs) as the tool for building . NET libraries that work everywhere (cross-platform)*

## Blazor Web Assembly in Browser Runtime

![](https://user-images.githubusercontent.com/6631390/86378114-44c29180-bc57-11ea-9696-c407ebee91f8.png)

## Blazor Web Assembly Limitations

* Blazor WASM is HTML framework similar to something like Angular, Vue, etc. for rendering Razor pages on the client side. It’s not a generic engine to execute .NET code in the browser 
* There are limitations in Web Assembly that require quite a bit of JavaScript interop in order to access the DOM or other Web APIs as Web Assembly can’t access the DOM or APIs directly
* The payload is not negligible as it requires the load of a sizable WASM module, plus the JavaScript loader and interop handler code that has to be loaded into the browser for each page. 
* The load is getting comparable with the loads required by full frameworks like Angular, Ember, or Aurelia
* Performance is still inferior to JavaScript framework, but the advantage of using C# and .NET libs may overtake that
