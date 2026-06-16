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
  - ZhrWsSettings _settings
  + CreateClient() : ZhrBaseClient
}

class ZhrWsDeltasClientFactory implements IZhrWsClientFactory {
  - ZhrWsSettings _settings
  + CreateClient() : ZhrBaseClient
}

class ZhrWsDescodifClientFactory implements IZhrWsClientFactory {
  - ZhrWsSettings _settings
  + CreateClient() : ZhrBaseClient
}

' ========== BINDING FACTORY ==========
interface IBindingFactory {
  + CreateBinding() : CustomBinding
}

class BindingFactory implements IBindingFactory {
  - WcfBindingSettings _bindingSettings
  + BindingFactory(ZhrWsSettings settings)
  + CreateBinding() : CustomBinding
}

' ========== CONFIGURATION ==========
class ZhrWsSettings {
  + Empresa : string
  + Endpoints : ZhrEndpointSettings
  + Auth : ZhrAuthenticationSettings
  + Binding : WcfBindingSettings
}

class WcfBindingSettings {
  + SoapVersion : string
  + Encoding : string
  + MaxBufferSize : int
  + MaxReceivedMessageSize : long
  + DecompressionEnabled : bool
  + UseDefaultWebProxy : bool
  + ReceiveTimeoutSeconds : int
  + SendTimeoutSeconds : int
  + OpenTimeoutSeconds : int
  + CloseTimeoutSeconds : int
}

note right of ZhrWsSettings
  **Security Warning:** `ClientPassword` must be retrieved
  from a secure secret provider (e.g., Environment Variables,
  Key Vault) and never stored in plain text configuration files.
end note

ZhrWsSettings "1" *-- "1" WcfBindingSettings : contains
BindingFactory ..> ZhrWsSettings : depends on (constructor)
ZHR_WSClient --|> ZhrBaseClient
ZHR_WSDeltasClient --|> ZhrBaseClient
ZHR_WSDescodifClient --|> ZhrBaseClient

ZhrWsClientFactory --> ZhrWsSettings : reads
ZhrWsClientFactory --> IBindingFactory : uses
ZhrWsClientFactory ..> ZHR_WSClient : creates

ZhrWsDeltasClientFactory --> ZhrWsSettings : reads
ZhrWsDeltasClientFactory --> IBindingFactory : uses
ZhrWsDeltasClientFactory ..> ZHR_WSDeltasClient : creates

ZhrWsDescodifClientFactory --> ZhrWsSettings : reads
ZhrWsDescodifClientFactory --> IBindingFactory : uses
ZhrWsDescodifClientFactory ..> ZHR_WSDescodifClient : creates

BindingFactory --> WcfBindingSettings : reads

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
