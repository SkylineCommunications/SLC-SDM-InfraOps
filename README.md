# SDM InfraOps

📋 **About**  
**Skyline.DataMiner.SDM.InfraOps** is a Standard Data Model (SDM) package that provides a strongly‑typed, high‑performance API for managing your infrastructure within the DataMiner ecosystem.  
It covers two primary functional domains:

- **Asset Management** — Assets, Ports, Classes, Types, Holders, Locations  
- **Facility Management** — Facility metadata, geospatial attributes, hierarchical facility structures  

---

## 🚀 Key Features

### Strongly‑Typed Infrastructure Modeling
- 🔧 Rich models for Assets, Device Types, Data Ports, Power Ports, Locations, and more  
- 🧩 Clear domain boundaries for Asset & Facility management  
- 🏷️ Reference‑safe relationships between SDM entities   

### Multi‑Context Integration
- 🤖 Automation helpers  
  - engine.GetAssetManagementApiHelper()  
  - engine.GetFacilityManagementApiHelper()
- 🔌 Protocol helpers  
  - protocol.GetAssetManagementApiHelper()  
  - protocol.GetFacilityManagementApiHelper()
- 🧠 GQI helpers  
  - args.DMS.GetAssetManagementApiHelper()  
  - args.DMS.GetFacilityManagementApiHelper()

### Developer Experience
- 🧪 Extensive unit test suite (CRUD, Filters, Paging, Bulk)  
- 🧱 Eight NuGets separating common logic from host‑specific entry points  
- 🔧 Customizable repositories via Extensions and Middleware partial classes  

## 📦 NuGet Packages

### Asset Management
- Skyline.DataMiner.SDM.AssetManagement.Common  
- Skyline.DataMiner.SDM.AssetManagement.Automation  
- Skyline.DataMiner.SDM.AssetManagement.Protocol  
- Skyline.DataMiner.SDM.AssetManagement.GQI  

### Facility Management
- Skyline.DataMiner.SDM.FacilityManagement.Common  
- Skyline.DataMiner.SDM.FacilityManagement.Automation  
- Skyline.DataMiner.SDM.FacilityManagement.Protocol  
- Skyline.DataMiner.SDM.FacilityManagement.GQI  


## 🧬 Model Schema

### Asset Models

Asset, DeviceType, DataPort, PowerPort schemas...

## 🔧 Core Usage

### ▶️ Create
```csharp
var api = engine.GetAssetManagementApiHelper();
var asset = new Asset { Name = "Core Switch A1" };
var created = api.Assets.Create(asset);
```

### 🔄 CreateOrUpdate
```csharp
api.Assets.CreateOrUpdate(asset);
api.Assets.CreateOrUpdate(new[] { assetA, assetB });
```

### 📄 ReadPaged
```csharp
var results = api.Assets.ReadPaged(filter, 100);
```

### ❌ Delete
```csharp
api.Assets.Delete(assetToDelete);
```

### 📝 Best Practices
Use bulk operations, typed exposers, paged reads, GUIDs, helper caching, etc.

## 🤝 Contributing
We welcome contributions from the community!
1. Open an Issue
2. Fork the Repository
3. Follow Coding Guidelines
4. Submit a Pull Request


## About DataMiner
https://aka.dataminer.services/about-dataminer

## About Skyline Communications
https://aka.dataminer.services/about-skyline
