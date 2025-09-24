export interface LookupApiQuery {
    controller: string
    language: string
    unit: string
    userId: string
    filter:
    {
        field: string
        value: string
        operator: string
    }[]
}

export interface LookupApiResponse {
    primaryKey: string[]
    fields: {
        field: string
        header: string
    }[]
    datas: Record<string, string>[]
}