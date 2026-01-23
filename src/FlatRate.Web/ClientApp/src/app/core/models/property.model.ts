export interface Property {
  id: string;
  name: string;
  address: string;
  defaultElectricityRate: number | null;
  defaultWaterRateTier1: number | null;
  defaultWaterRateTier2: number | null;
  defaultWaterRateTier3: number | null;
  defaultSanitationRateTier1: number | null;
  defaultSanitationRateTier2: number | null;
  defaultSanitationRateTier3: number | null;
  createdAt: string;
  updatedAt: string | null;
}

export interface CreatePropertyRequest {
  name: string;
  address: string;
}

export interface UpdatePropertyRequest {
  name: string;
  address: string;
}

export interface SetPropertyRatesRequest {
  electricityRate: number | null;
  waterRateTier1: number | null;
  waterRateTier2: number | null;
  waterRateTier3: number | null;
  sanitationRateTier1: number | null;
  sanitationRateTier2: number | null;
  sanitationRateTier3: number | null;
}
