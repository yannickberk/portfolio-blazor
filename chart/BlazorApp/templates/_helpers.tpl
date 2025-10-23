{{- define "blazorapp.name" -}}
{{- default .Chart.Name .Values.nameOverride | trunc 63 -}}
{{- end -}}

{{- define "blazorapp.fullname" -}}
{{- printf "%s" (include "blazorapp.name" .) -}}
{{- end -}}
