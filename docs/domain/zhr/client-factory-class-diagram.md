# Zhr WS Client Factory Class Diagram

```plantuml
@startuml ZHR WS Client Factory
title ZHR WS Client Factory Pattern Architecture

' ========== BASE CLIENT ==========
abstract class ZhrBaseClient

' ========== WCF CLIENTS ==========
class ZHR_WSClient {
  + ZhrWsOpAsync(request) : Task<ZhrWsOpResponse>
}

class ZHR_WSDeltasClient {
  + GetDeltasAsync(numsap) : Task<DeltasResponse>
}

class ZHR_WSDescodifClient {
  + DescodificarAsync(codigo) : Task<DescodificaResponse>
}

' ========== FACTORY INTERFACE ==========
interface IZhrWsClientFactory {
  + CreateClient() : ZhrBaseClient
}

' ========== FACTORY IMPLEMENTATIONS ==========
class ZhrWsClientFactory implements IZhrWsClientFactory {
  - DataSourceSettings _settings
  + CreateClient() : ZhrBaseClient
}

class ZhrWsDeltasClientFactory implements IZhrWsClientFactory {
  - DataSourceSettings _settings
  + CreateClient() : ZhrBaseClient
}

class ZhrWsDescodifClientFactory implements IZhrWsClientFactory {
  - DataSourceSettings _settings
  + CreateClient() : ZhrBaseClient
}

' ========== BINDING FACTORY ==========
class SoapBindingFactory <<utility>> {
  + {static} CreateDefaultBinding() : CustomBinding
}

' ========== CONFIGURATION ==========
class DataSourceSettings {
  + OutputUrl : string
  + DeltasUrl : string
  + Empresa : string
  + ClientUsername : string
  + ClientPassword : string
}

note right of DataSourceSettings
  **Security Warning:** `ClientPassword` must be retrieved
  from a secure secret provider (e.g., Environment Variables,
  Key Vault) and never stored in plain text configuration files.
end note

' ========== RELATIONS ==========
ZHR_WSClient --|> ZhrBaseClient
ZHR_WSDeltasClient --|> ZhrBaseClient
ZHR_WSDescodifClient --|> ZhrBaseClient

ZhrWsClientFactory --> DataSourceSettings : reads
ZhrWsClientFactory --> SoapBindingFactory : uses
ZhrWsClientFactory ..> ZHR_WSClient : creates

ZhrWsDeltasClientFactory --> DataSourceSettings : reads
ZhrWsDeltasClientFactory --> SoapBindingFactory : uses
ZhrWsDeltasClientFactory ..> ZHR_WSDeltasClient : creates

ZhrWsDescodifClientFactory --> DataSourceSettings : reads
ZhrWsDescodifClientFactory --> SoapBindingFactory : uses
ZhrWsDescodifClientFactory ..> ZHR_WSDescodifClient : creates

note right of ZhrBaseClient
  Marker class - base for all ZHR clients
end note

note right of IZhrWsClientFactory
  Single interface for all factories
  Returns ZhrBaseClient (polymorphic)
  Type-safe via ZhrBaseClient constraint
end note

@enduml
```
