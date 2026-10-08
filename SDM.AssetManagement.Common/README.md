# Skyline.DataMiner.SDM.InfraOps

## About

Standard API for InfraOps

### History instance identifiers

`HistoryInfo.ModifiedInstanceID` remains an `ISdmObjectReference<ISdmObject>`, but its identifier is read, written, and filtered as a string in DOM. History Info section `cec87feb-30d0-4270-8bf8-7e8282d0513c`, Instance ID field `4d913684-12ec-4666-98a2-5e4db7fb5acf`, is `System.String`; do not convert its value to a GUID. The History Job field remains GUID-backed.

The repository does not contain the external code generator's schema or templates, and its source-generator package reference is disabled. When regenerating the checked-in History repository, preserve this string storage mapping and run the History repository tests. The mock section definition already declares Instance ID as `typeof(string)`.

### About DataMiner

DataMiner is a transformational platform that provides vendor-independent control and monitoring of devices and services. Out of the box and by design, it addresses key challenges such as security, complexity, multi-cloud, and much more. It has a pronounced open architecture and powerful capabilities enabling users to evolve easily and continuously.

The foundation of DataMiner is its powerful and versatile data acquisition and control layer. With DataMiner, there are no restrictions to what data users can access. Data sources may reside on premises, in the cloud, or in a hybrid setup.

A unique catalog of 7000+ connectors already exist. In addition, you can leverage DataMiner Development Packages to build you own connectors (also known as "protocols" or "drivers").

> **Note**
> See also: [About DataMiner](https://aka.dataminer.services/about-dataminer).

### About Skyline Communications

At Skyline Communications, we deal in world-class solutions that are deployed by leading companies around the globe. Check out [our proven track record](https://aka.dataminer.services/about-skyline) and see how we make our customers' lives easier by empowering them to take their operations to the next level.

<!-- Uncomment below and add more info to provide more information about how to use this package. -->
<!-- ## Getting Started -->
