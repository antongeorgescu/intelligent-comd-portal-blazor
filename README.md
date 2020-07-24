# Configuration and Operations Monitoring Dashboard (COMD)
## Design Option: Blazor with Web Assembly

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



