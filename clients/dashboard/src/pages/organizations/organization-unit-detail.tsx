import {useMemo, useState, type FormEvent} from "react";
import {Link, useParams} from "react-router-dom";
import {useMutation, useQuery, useQueryClient} from "@tanstack/react-query";
import {ArrowLeft, Briefcase, History, Pencil, Plus, XCircle} from "lucide-react";
import {toast} from "sonner";
import {
    allocatePosition,
    changePositionAllocation,
    endPositionAllocation,
    listCurrentPositionAllocations,
    listOrganizationUnits,
    listPositionAllocationHistory,
    type OrganizationUnitPositionAllocationDto
} from "@/api/organizations";
import {listPositions} from "@/api/positions";
import {Button} from "@/components/ui/button";
import {Card, CardContent, CardHeader, CardTitle} from "@/components/ui/card";
import {
    Dialog,
    DialogBody,
    DialogClose,
    DialogContent,
    DialogDescription,
    DialogFooter,
    DialogHeader,
    DialogTitle
} from "@/components/ui/dialog";
import {Input} from "@/components/ui/input";
import {Label} from "@/components/ui/label";

const STORAGE_KEY = "selected_organization_id";

export function OrganizationUnitDetailPage() {
    const {unitId = ""} = useParams();
    const organizationId = localStorage.getItem(STORAGE_KEY);
    const client = useQueryClient();
    const [allocateOpen, setAllocateOpen] = useState(false);
    const [changing, setChanging] = useState<OrganizationUnitPositionAllocationDto | null>(null);
    const [historyPositionId, setHistoryPositionId] = useState<string | null>(null);
    const [positionId, setPositionId] = useState("");
    const [headCount, setHeadCount] = useState("");

    const unitQuery = useQuery({
        queryKey: ["organizations", "units", organizationId],
        queryFn: () => listOrganizationUnits(organizationId)
    });
    const unit = useMemo(() => findUnit(unitQuery.data ?? [], unitId), [unitId, unitQuery.data]);
    const allocationsKey = ["organizations", "units", unitId, "allocations"] as const;
    const allocationsQuery = useQuery({
        queryKey: allocationsKey,
        queryFn: () => listCurrentPositionAllocations(unitId),
        enabled: Boolean(unitId)
    });
    const positionsQuery = useQuery({
        queryKey: ["organizations", "positions", organizationId],
        queryFn: () => listPositions(organizationId)
    });
    const historyQuery = useQuery({
        queryKey: ["organizations", "units", unitId, "positions", historyPositionId, "history"],
        queryFn: () => listPositionAllocationHistory(unitId, historyPositionId!),
        enabled: Boolean(historyPositionId)
    });
    const refresh = () => client.invalidateQueries({queryKey: allocationsKey});
    const allocate = useMutation({
        mutationFn: () => allocatePosition(unitId, {
            positionId,
            headCount: headCount ? Number(headCount) : null
        }), onSuccess: () => {
            toast.success("Position allocated");
            setAllocateOpen(false);
            setPositionId("");
            setHeadCount("");
            refresh();
        }, onError: () => toast.error("Could not allocate position")
    });
    const change = useMutation({
        mutationFn: () => changePositionAllocation(changing!.id, headCount ? Number(headCount) : null),
        onSuccess: () => {
            toast.success("Allocation changed");
            setChanging(null);
            setHeadCount("");
            refresh();
        },
        onError: () => toast.error("Could not change allocation")
    });
    const end = useMutation({
        mutationFn: endPositionAllocation, onSuccess: () => {
            toast.success("Allocation ended");
            refresh();
        }, onError: () => toast.error("Could not end allocation")
    });
    const availablePositions = (positionsQuery.data ?? []).filter((position) => !(allocationsQuery.data ?? []).some((allocation) => allocation.position.referenceId === position.referenceId));
    const submitAllocation = (event: FormEvent) => {
        event.preventDefault();
        if (positionId) allocate.mutate();
    };
    const submitChange = (event: FormEvent) => {
        event.preventDefault();
        if (changing) change.mutate();
    };

    return <div className="space-y-6">
        <div><Button asChild variant="ghost" size="sm"><Link to="/organizations/units"><ArrowLeft className="size-4"/>Organization
            structure</Link></Button>
            <div className="mt-3 flex flex-col gap-3 sm:flex-row sm:items-end sm:justify-between">
                <div><p className="text-sm text-[var(--color-muted-foreground)]">Organization unit</p><h1
                    className="text-display text-2xl font-semibold">{unit?.name ?? "Organization unit"}</h1><p
                    className="mt-1 text-sm text-[var(--color-muted-foreground)]">{unit?.description || "No description"}</p>
                </div>
                <Button onClick={() => setAllocateOpen(true)}><Plus className="size-4"/>Allocate position</Button></div>
        </div>
        <div className="grid gap-4 sm:grid-cols-3"><Info label="Code" value={unit?.code}/><Info label="Reference ID"
                                                                                                value={unit?.referenceId}/><Info
            label="Organization" value={organizationId ?? "Default organization"}/></div>
        <Card><CardHeader className="flex-row items-center justify-between">
            <div><CardTitle className="flex items-center gap-2"><Briefcase className="size-5"/>Current position
                allocations</CardTitle><p className="mt-1 text-sm text-[var(--color-muted-foreground)]">Active
                allocations for this organization unit.</p></div>
        </CardHeader><CardContent>{allocationsQuery.isLoading ?
            <p className="py-8 text-center text-sm text-[var(--color-muted-foreground)]">Loading
                allocations…</p> : (allocationsQuery.data ?? []).length === 0 ?
                <p className="py-8 text-center text-sm text-[var(--color-muted-foreground)]">No current position
                    allocations.</p> :
                <div className="divide-y divide-[var(--color-border)]">{allocationsQuery.data?.map((allocation) => <div
                    key={allocation.id} className="flex flex-col gap-3 py-3 sm:flex-row sm:items-center">
                    <div className="min-w-0 flex-1"><p className="font-medium">{allocation.position.name} <span
                        className="ml-1 font-mono text-xs text-[var(--color-muted-foreground)]">{allocation.position.code}</span>
                    </p><p
                        className="text-xs text-[var(--color-muted-foreground)]">Effective {formatDate(allocation.effectiveFrom)} ·
                        Headcount: {allocation.headCount ?? "Not set"}</p></div>
                    <div className="flex gap-1"><Button variant="ghost" size="sm" onClick={() => {
                        setChanging(allocation);
                        setHeadCount(allocation.headCount?.toString() ?? "");
                    }}><Pencil className="size-4"/>Change</Button><Button variant="ghost" size="sm"
                                                                          onClick={() => setHistoryPositionId(allocation.position.referenceId)}><History
                        className="size-4"/>History</Button><Button variant="ghost" size="sm" disabled={end.isPending}
                                                                    onClick={() => {
                                                                        if (window.confirm(`End ${allocation.position.name} allocation now?`)) end.mutate(allocation.id);
                                                                    }}><XCircle
                        className="size-4 text-[var(--color-destructive)]"/>End</Button></div>
                </div>)}</div>}</CardContent></Card><Dialog open={allocateOpen}
                                                            onOpenChange={setAllocateOpen}><DialogContent>
        <form onSubmit={submitAllocation}><DialogHeader><DialogTitle>Allocate position</DialogTitle><DialogDescription>Allocate
            an existing position to {unit?.name ?? "this unit"}.</DialogDescription></DialogHeader><DialogBody
            className="space-y-4">
            <div className="space-y-2"><Label htmlFor="position">Position</Label><select id="position"
                                                                                         value={positionId}
                                                                                         onChange={(event) => setPositionId(event.target.value)}
                                                                                         required
                                                                                         className="h-9 w-full rounded-md border border-[var(--color-input)] bg-transparent px-3 text-sm">
                <option value="">Select a position</option>
                {availablePositions.map((position) => <option key={position.referenceId}
                                                              value={position.referenceId}>{position.name} ({position.code})</option>)}
            </select></div>
            <Headcount value={headCount} onChange={setHeadCount}/></DialogBody><DialogFooter><DialogClose
            asChild><Button type="button" variant="outline">Cancel</Button></DialogClose><Button type="submit"
                                                                                                 disabled={allocate.isPending || !positionId}>{allocate.isPending ? "Allocating…" : "Allocate"}</Button></DialogFooter>
        </form>
    </DialogContent></Dialog><Dialog open={Boolean(changing)}
                                     onOpenChange={(open) => !open && setChanging(null)}><DialogContent>
        <form onSubmit={submitChange}><DialogHeader><DialogTitle>Change allocation</DialogTitle><DialogDescription>This
            ends the current allocation and creates a new history record effective
            now.</DialogDescription></DialogHeader><DialogBody><Headcount value={headCount}
                                                                          onChange={setHeadCount}/></DialogBody><DialogFooter><DialogClose
            asChild><Button type="button" variant="outline">Cancel</Button></DialogClose><Button type="submit"
                                                                                                 disabled={change.isPending}>{change.isPending ? "Changing…" : "Save change"}</Button></DialogFooter>
        </form>
    </DialogContent></Dialog><Dialog open={Boolean(historyPositionId)}
                                     onOpenChange={(open) => !open && setHistoryPositionId(null)}><DialogContent><DialogHeader><DialogTitle>Allocation
        history</DialogTitle><DialogDescription>Historical changes for this position in this organization
        unit.</DialogDescription></DialogHeader><DialogBody>{historyQuery.isLoading ?
        <p className="text-sm text-[var(--color-muted-foreground)]">Loading history…</p> :
        <div className="space-y-3">{historyQuery.data?.map((item) => <div key={item.id}
                                                                          className="rounded-lg border border-[var(--color-border)] p-3 text-sm">
            <p className="font-medium">Headcount: {item.headCount ?? "Not set"}</p><p
            className="text-xs text-[var(--color-muted-foreground)]">{formatDate(item.effectiveFrom)} — {item.effectiveTo ? formatDate(item.effectiveTo) : "Current"}</p>
        </div>)}</div>}</DialogBody><DialogFooter><DialogClose asChild><Button
        variant="outline">Close</Button></DialogClose></DialogFooter></DialogContent></Dialog></div>;
}

function findUnit(units: { referenceId: string; children?: unknown[] }[], referenceId: string): {
    referenceId: string;
    name: string;
    code: string;
    description?: string | null
} | undefined {
    for (const unit of units) {
        if (unit.referenceId === referenceId) return unit as {
            referenceId: string;
            name: string;
            code: string;
            description?: string | null
        };
        const child = findUnit((unit.children ?? []) as { referenceId: string; children?: unknown[] }[], referenceId);
        if (child) return child;
    }
    return undefined;
}

function Info({label, value}: { label: string; value?: string }) {
    return <Card><CardContent className="p-4"><p className="text-xs text-[var(--color-muted-foreground)]">{label}</p><p
        className="mt-1 truncate font-mono text-sm">{value || "—"}</p></CardContent></Card>;
}

function Headcount({value, onChange}: { value: string; onChange: (value: string) => void }) {
    return <div className="space-y-2"><Label htmlFor="headcount">Headcount</Label><Input id="headcount" type="number"
                                                                                         min="1" placeholder="Not set"
                                                                                         value={value}
                                                                                         onChange={(event) => onChange(event.target.value)}/>
    </div>;
}

function formatDate(value: string) {
    return new Intl.DateTimeFormat(undefined, {dateStyle: "medium"}).format(new Date(value));
}
