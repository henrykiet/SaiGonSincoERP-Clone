export interface Unit {
  unitCode: string;
  unitName: string;
  unitName2: string;
}

export interface UnitResponse {
  success: boolean;
  data: Unit[];
  message: string;
} 