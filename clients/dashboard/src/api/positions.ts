import { apiFetch } from "@/lib/api-client";

export type PositionDto = {
  referenceId: string;
  code: string;
  name: string;
  description?: string | null;
  createdOnUtc?: string;
  createdBy?: string | null;
  lastModifiedOnUtc?: string | null;
  lastModifiedBy?: string | null;
  isDeleted?: boolean;
};

export type PositionInput = {
  code: string;
  name: string;
  description?: string | null;
};

export async function listPositions(organizationId?: string | null): Promise<PositionDto[]> {
  const url = organizationId
    ? `/api/v1/organizations/${encodeURIComponent(organizationId)}/positions`
    : "/api/v1/organizations/positions";
  return apiFetch<PositionDto[]>(url);
}

export async function createPosition(input: PositionInput & { organizationId?: string | null }): Promise<PositionDto> {
  return apiFetch<PositionDto>("/api/v1/organizations/positions", {
    method: "POST",
    body: JSON.stringify(input),
  });
}

export async function updatePosition(referenceId: string, input: PositionInput): Promise<PositionDto> {
  return apiFetch<PositionDto>(`/api/v1/organizations/positions/${encodeURIComponent(referenceId)}`, {
    method: "PUT",
    body: JSON.stringify(input),
  });
}

export async function deletePosition(referenceId: string): Promise<void> {
  await apiFetch<PositionDto>(`/api/v1/organizations/positions/${encodeURIComponent(referenceId)}`, {
    method: "DELETE",
  });
}
