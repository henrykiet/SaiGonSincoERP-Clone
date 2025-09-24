export interface ListApiResponse {
  controller: string
  tableName: string
  primaryKey: string[]
  language: string
  unit: string
  idVC: string
  type: string
  action: string
  sort: string
  userId: string
  data: Record<string, string>[]
  total: number
  page: number
  pageSize: number
  isFileHandle? : boolean
}

export interface FormIdParams {
  controller: string
  tableName: string
  primaryKey: string[]
  type: string
  action: string
  sort: string
  language: string
  unit: string
  idVC: string
  userId: string
  listTable: string[]
}

export interface DetailApiQuery {
  controller: string
  formId: string
  primaryKey: string[]
  value: string[]
  type: string
  action: string
  language: string
  unit: string
  idVC: string
  userId: string
  VCDate: string
  listTable: string[]
  data?: any,
  isFileHandle?: boolean
  dataProcessing?: { actions: { post: any[] } }
}

export interface ListApiQuery {
  formId: DetailApiQuery
  filter?: FilterCondition[]
  page?: number
  pageSize?: number
  sort?: string
}

export interface FilterCondition {
  id: string
  field: string
  operator: string
  value: any
  columnType?: string
}

export interface ApiResponse {
  Data: any
}

export interface GirdInitData {
  id: string
  title?: string
  headers: GirdHeader[]
  query: ListApiQuery
  sort: string
  actions: GridAction[]
  mode?: string
  width?: string
}

export interface GridAction {
  controller: string
  id: string
  label: string
  target: string
  color: string
}

export interface GirdHeader {
  key: string
  label: string
  type: string
  sortable?: boolean
  hidden?: boolean
  width?: string
}
