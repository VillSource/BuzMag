import { apiFetch } from "@/lib/api-client";

export type OrganizationUnitDto = { id: string; parenId?: string | null; path?: string; name: string; code: string; referenceId?: string | null; description?: string | null; children?: OrganizationUnitDto[] };
export type OrganizationUnitInput = { name: string; code: string; description?: string | null };

export async function listOrganizationUnits(): Promise<OrganizationUnitDto[]> { return apiFetch<OrganizationUnitDto[]>("/api/v1/organizations/units"); }
export async function createOrganizationUnit(input: OrganizationUnitInput & { parentId?: string | null }): Promise<OrganizationUnitDto> { return apiFetch<OrganizationUnitDto>("/api/v1/organizations/units", { method: "POST", body: JSON.stringify(input) }); }
export async function updateOrganizationUnit(id: string, input: OrganizationUnitInput): Promise<OrganizationUnitDto> { return apiFetch<OrganizationUnitDto>(`/api/v1/organizations/units/${encodeURIComponent(id)}`, { method: "PUT", body: JSON.stringify(input) }); }
export async function deleteOrganizationUnit(id: string): Promise<void> { await apiFetch<OrganizationUnitDto>(`/api/v1/organizations/units/${encodeURIComponent(id)}`, { method: "DELETE" }); }
export async function moveOrganizationUnit(id: string, newParentId: string | null): Promise<OrganizationUnitDto> { return apiFetch<OrganizationUnitDto>(`/api/v1/organizations/units/${encodeURIComponent(id)}/move`, { method: "POST", body: JSON.stringify({ newParentId, newOrganizationId: null }) }); }
