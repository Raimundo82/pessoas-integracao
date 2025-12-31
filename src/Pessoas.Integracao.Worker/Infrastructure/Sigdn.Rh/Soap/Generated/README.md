# Generated Services

## SIGDN-RH Output webservice generated code

```sh
dotnet dotnet-svcutil http://esb-lb-soa.marinha.pt:8301/ZHR_EPR?wsdl \
-d src/Pessoas.Integracao.Worker/Infrastucture/Sigdn.Rh/Soap/Generated/Output/ \
-o OutputServices  \
--namespace "*,Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Generated.Output"
```

## SIGDN-RH Deltas webservice generated code

```sh
dotnet dotnet-svcutil http://esb-lb-soa.marinha.pt:8301/ZHR_deltas_EPR?wsdl \
-d src/Pessoas.Integracao.Worker/Infrastucture/Sigdn.Rh/Soap/Generated/Deltas/ \
-o DeltasService  \
--namespace "*,Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Generated.Deltas"
```

## SIGDN-RH Descodif webservice generated code

```sh
dotnet dotnet-svcutil http://esb-lb-soa.marinha.pt:8301/ZHR_descodif_EPR?wsdl \
-d src/Pessoas.Integracao.Worker/Infrastucture/Sigdn.Rh/Soap/Generated/Descodificadoras/ \
-o DescodificadorasServices  \
--namespace "*,Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Generated.Descodifacadoras"
```
