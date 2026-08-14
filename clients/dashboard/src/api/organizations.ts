import {apiFetch} from "@/lib/api-client";

export type OrganizationDto = {
    referenceId: string;
    isDefault: boolean;
    createdOnUtc?: string;
    createdBy?: string | null;
    lastModifiedOnUtc?: string | null;
    lastModifiedBy?: string | null;
    isDeleted?: boolean;
    deletedOnUtc?: string | null;
    deletedBy?: string | null;
    units?: OrganizationUnitDto[];
};

export type OrganizationUnitDto = {
    referenceId: string;
    path?: string;
    name: string;
    code: string;
    description?: string | null;
    children?: OrganizationUnitDto[]
};
export type OrganizationUnitInput = { name: string; code: string; description?: string | null };
export type PositionDto = { referenceId: string; name: string; code: string; description?: string | null };
export type OrganizationUnitPositionAllocationDto = {
    id: string;
    organizationUnitReferenceId: string;
    position: PositionDto;
    headCount?: number | null;
    effectiveFrom: string;
    effectiveTo?: string | null
};

export async function listOrganizations(): Promise<OrganizationDto[]> {
    return apiFetch<OrganizationDto[]>("/api/v1/organizations");
}

export async function listOrganizationUnits(organizationId?: string | null): Promise<OrganizationUnitDto[]> {
    const url = organizationId
        ? `/api/v1/organizations/${encodeURIComponent(organizationId)}/units`
        : "/api/v1/organizations/units";
    return apiFetch<OrganizationUnitDto[]>(url);
}

export async function createOrganizationUnit(input: OrganizationUnitInput & {
    parentId?: string | null;
    organizationId?: string | null
}): Promise<OrganizationUnitDto> {
    return apiFetch<OrganizationUnitDto>("/api/v1/organizations/units", {method: "POST", body: JSON.stringify(input)});
}

export async function updateOrganizationUnit(referenceId: string, input: OrganizationUnitInput): Promise<OrganizationUnitDto> {
    return apiFetch<OrganizationUnitDto>(`/api/v1/organizations/units/${encodeURIComponent(referenceId)}`, {
        method: "PUT",
        body: JSON.stringify(input)
    });
}

export async function deleteOrganizationUnit(referenceId: string): Promise<void> {
    await apiFetch<OrganizationUnitDto>(`/api/v1/organizations/units/${encodeURIComponent(referenceId)}`, {method: "DELETE"});
}

export async function moveOrganizationUnit(referenceId: string, newParentId: string | null): Promise<OrganizationUnitDto> {
    return apiFetch<OrganizationUnitDto>(`/api/v1/organizations/units/${encodeURIComponent(referenceId)}/move`, {
        method: "POST",
        body: JSON.stringify({newParentId, newOrganizationId: null})
    });
}

export async function listCurrentPositionAllocations(unitId: string): Promise<OrganizationUnitPositionAllocationDto[]> {
    return apiFetch(`/api/v1/organizations/units/${encodeURIComponent(unitId)}/position-allocations`);
}

export async function listPositionAllocationHistory(unitId: string, positionId: string): Promise<OrganizationUnitPositionAllocationDto[]> {
    return apiFetch(`/api/v1/organizations/units/${encodeURIComponent(unitId)}/positions/${encodeURIComponent(positionId)}/allocation-history`);
}

export async function allocatePosition(unitId: string, input: {
    positionId: string;
    headCount?: number | null
}): Promise<OrganizationUnitPositionAllocationDto> {
    return apiFetch(`/api/v1/organizations/units/${encodeURIComponent(unitId)}/position-allocations`, {
        method: "POST",
        body: JSON.stringify(input)
    });
}

export async function changePositionAllocation(allocationId: string, headCount?: number | null): Promise<OrganizationUnitPositionAllocationDto> {
    return apiFetch(`/api/v1/organizations/position-allocations/${encodeURIComponent(allocationId)}/change`, {
        method: "POST",
        body: JSON.stringify({headCount})
    });
}

export async function endPositionAllocation(allocationId: string): Promise<OrganizationUnitPositionAllocationDto> {
    return apiFetch(`/api/v1/organizations/position-allocations/${encodeURIComponent(allocationId)}/end`, {method: "POST"});
}

