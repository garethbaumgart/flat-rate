import { PropertyRole } from './property.model';

export interface Collaborator {
  userId: string | null;
  email: string | null;
  name: string | null;
  role: PropertyRole;
  isPending: boolean;
  createdAt: string;
  acceptedAt: string | null;
}

export interface InviteRequest {
  email: string;
  role?: PropertyRole;
}
