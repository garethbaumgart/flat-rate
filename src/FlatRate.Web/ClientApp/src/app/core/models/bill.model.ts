export interface Bill {
  id: string;
  invoiceNumber: string;
  propertyId: string;
  periodStart: string;
  periodEnd: string;
  electricityReading: MeterReading;
  waterReading: MeterReading;
  sanitationReading: MeterReading;
  electricityCost: number;
  waterCost: number;
  sanitationCost: number;
  subtotal: number;
  vatAmount: number;
  total: number;
  createdAt: string;
}

export interface MeterReading {
  opening: number;
  closing: number;
  unitsUsed: number;
}

export interface CreateBillRequest {
  propertyId: string;
  periodStart: string;
  periodEnd: string;
  electricityReadingOpening: number;
  electricityReadingClosing: number;
  waterReadingOpening: number;
  waterReadingClosing: number;
  sanitationReadingOpening: number;
  sanitationReadingClosing: number;
  electricityRate: number;
  waterRateTier1: number;
  waterRateTier2: number;
  waterRateTier3: number;
  sanitationRateTier1: number;
  sanitationRateTier2: number;
  sanitationRateTier3: number;
}

export interface BillPreview {
  electricityUnits: number;
  waterUnits: number;
  sanitationUnits: number;
  electricityCost: number;
  waterCost: number;
  sanitationCost: number;
  subtotal: number;
  vatAmount: number;
  total: number;
}
