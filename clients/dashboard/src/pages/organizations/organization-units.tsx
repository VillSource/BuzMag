import { useEffect, useMemo, useState, type FormEvent } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Building2, ChevronDown, ChevronRight, GitBranch, Move, Pencil, Plus, Trash2 } from "lucide-react";
import { toast } from "sonner";
import {
  createOrganizationUnit,
  deleteOrganizationUnit,
  listOrganizationUnits,
  moveOrganizationUnit,
  updateOrganizationUnit,
  type OrganizationUnitDto,
  type OrganizationUnitInput,
} from "@/api/organizations";
import { Button } from "@/components/ui/button";
import {
  EntityEmpty,
  EntityListCard,
  EntityListHeader,
  EntityListLoading,
  EntityListRow,
  EntityPageHeader,
} from "@/components/list";
import { Dialog, DialogBody, DialogClose, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";

type UnitNode = Omit<OrganizationUnitDto, "children"> & {
  parentId: string | null;
  children: UnitNode[];
};
type UnitForm = { name: string; code: string; description: string };

const EMPTY_FORM: UnitForm = { name: "", code: "", description: "" };
const queryKey = ["organizations", "units"] as const;
const DESKTOP_COLUMNS = "grid-cols-[minmax(240px,1.3fr)_130px_minmax(150px,1fr)_178px]";

function toTree(units: OrganizationUnitDto[]): UnitNode[] {
  const byId = new Map<string, UnitNode>();
  const collect = (unit: OrganizationUnitDto, inheritedParent: string | null = null) => {
    byId.set(unit.id, { ...unit, parentId: unit.parenId ?? inheritedParent, children: [] });
    unit.children?.forEach((child) => collect(child, unit.id));
  };
  units.forEach((unit) => collect(unit));

  const roots: UnitNode[] = [];
  for (const unit of byId.values()) {
    const parent = unit.parentId ? byId.get(unit.parentId) : undefined;
    if (parent && parent.id !== unit.id) parent.children.push(unit);
    else roots.push(unit);
  }
  const sort = (items: UnitNode[]) => {
    items.sort((a, b) => a.name.localeCompare(b.name));
    items.forEach((item) => sort(item.children));
  };
  sort(roots);
  return roots;
}

function flatten(nodes: UnitNode[]): UnitNode[] {
  return nodes.flatMap((node) => [node, ...flatten(node.children)]);
}

function descendantIds(unit: UnitNode): Set<string> {
  return new Set(flatten(unit.children).map((child) => child.id));
}

export function OrganizationUnitsPage() {
  const client = useQueryClient();
  const query = useQuery({ queryKey, queryFn: listOrganizationUnits });
  const tree = useMemo(() => toTree(query.data ?? []), [query.data]);
  const allUnits = useMemo(() => flatten(tree), [tree]);
  const [expanded, setExpanded] = useState<Set<string>>(new Set());
  const [hasInitializedExpansion, setHasInitializedExpansion] = useState(false);
  const [formOpen, setFormOpen] = useState(false);
  const [moveOpen, setMoveOpen] = useState(false);
  const [editing, setEditing] = useState<UnitNode | null>(null);
  const [parentForNew, setParentForNew] = useState<UnitNode | null>(null);
  const [moving, setMoving] = useState<UnitNode | null>(null);
  const [form, setForm] = useState<UnitForm>(EMPTY_FORM);
  const [moveParentId, setMoveParentId] = useState("");

  useEffect(() => {
    if (!hasInitializedExpansion && allUnits.length > 0) {
      setExpanded(new Set(allUnits.map((unit) => unit.id)));
      setHasInitializedExpansion(true);
    }
  }, [allUnits, hasInitializedExpansion]);

  const refresh = () => client.invalidateQueries({ queryKey });
  const create = useMutation({
    mutationFn: createOrganizationUnit,
    onSuccess: () => { toast.success("Organization unit created"); setFormOpen(false); refresh(); },
    onError: () => toast.error("Could not create organization unit"),
  });
  const update = useMutation({
    mutationFn: ({ id, input }: { id: string; input: OrganizationUnitInput }) => updateOrganizationUnit(id, input),
    onSuccess: () => { toast.success("Organization unit updated"); setFormOpen(false); refresh(); },
    onError: () => toast.error("Could not update organization unit"),
  });
  const remove = useMutation({
    mutationFn: deleteOrganizationUnit,
    onSuccess: () => { toast.success("Organization unit deleted"); refresh(); },
    onError: () => toast.error("Could not delete organization unit"),
  });
  const move = useMutation({
    mutationFn: ({ id, parentId }: { id: string; parentId: string | null }) => moveOrganizationUnit(id, parentId),
    onSuccess: () => { toast.success("Organization unit moved"); setMoveOpen(false); refresh(); },
    onError: () => toast.error("Could not move organization unit"),
  });

  const openCreate = (parent: UnitNode | null = null) => {
    setEditing(null); setParentForNew(parent); setForm(EMPTY_FORM); setFormOpen(true);
  };
  const openEdit = (unit: UnitNode) => {
    setEditing(unit); setParentForNew(null);
    setForm({ name: unit.name, code: unit.code, description: unit.description ?? "" });
    setFormOpen(true);
  };
  const openMove = (unit: UnitNode) => {
    setMoving(unit); setMoveParentId(unit.parentId ?? ""); setMoveOpen(true);
  };
  const toggle = (id: string) => setExpanded((value) => {
    const next = new Set(value);
    if (next.has(id)) next.delete(id); else next.add(id);
    return next;
  });
  const submitForm = (event: FormEvent) => {
    event.preventDefault();
    const input = { name: form.name.trim(), code: form.code.trim(), description: form.description.trim() || null };
    if (!input.name || !input.code) return;
    if (editing) update.mutate({ id: editing.id, input });
    else create.mutate({ ...input, parentId: parentForNew?.id ?? null });
  };
  const submitMove = (event: FormEvent) => {
    event.preventDefault();
    if (moving) move.mutate({ id: moving.id, parentId: moveParentId || null });
  };
  const candidates = moving
    ? allUnits.filter((unit) => unit.id !== moving.id && !descendantIds(moving).has(unit.id))
    : [];

  return (
    <div className="space-y-4 sm:space-y-6">
      <EntityPageHeader
        icon={Building2}
        title="Organization structure"
        total={query.data ? allUnits.length : null}
        unit="unit"
        description="Manage organization units and their reporting structure."
      >
        <Button onClick={() => openCreate()}>
          <Plus className="size-4" />New root unit
        </Button>
      </EntityPageHeader>

      {query.isLoading ? <EntityListLoading rows={6} desktopColumns={DESKTOP_COLUMNS} />
        : tree.length === 0 ? <EntityEmpty icon={GitBranch} title="No organization units yet" body="Create a root unit to start building your structure." action={<Button onClick={() => openCreate()}><Plus className="size-4" />New root unit</Button>} />
        : <EntityListCard>
          <EntityListHeader className={DESKTOP_COLUMNS}>
            <span>Organization unit</span><span>Code</span><span>Reference ID</span><span>Actions</span>
          </EntityListHeader>
          {tree.map((unit, index) => <UnitRow key={unit.id} unit={unit} depth={0} isLast={index === tree.length - 1} expanded={expanded} onToggle={toggle} onCreate={openCreate} onEdit={openEdit} onMove={openMove} onDelete={(target) => { if (window.confirm(`Delete ${target.name}? This cannot be undone.`)) remove.mutate(target.id); }} deleting={remove.isPending} />)}
        </EntityListCard>}

      {query.isError && <p role="alert" className="text-sm text-[var(--color-destructive)]">The organization structure could not be loaded.</p>}

      <UnitFormDialog open={formOpen} onOpenChange={setFormOpen} editing={editing} parent={parentForNew} form={form} setForm={setForm} onSubmit={submitForm} busy={create.isPending || update.isPending} />
      <MoveUnitDialog open={moveOpen} onOpenChange={setMoveOpen} moving={moving} candidates={candidates} parentId={moveParentId} setParentId={setMoveParentId} onSubmit={submitMove} busy={move.isPending} />
    </div>
  );
}

function UnitRow({ unit, depth, isLast, expanded, onToggle, onCreate, onEdit, onMove, onDelete, deleting }: { unit: UnitNode; depth: number; isLast: boolean; expanded: Set<string>; onToggle: (id: string) => void; onCreate: (unit: UnitNode) => void; onEdit: (unit: UnitNode) => void; onMove: (unit: UnitNode) => void; onDelete: (unit: UnitNode) => void; deleting: boolean }) {
  const hasChildren = unit.children.length > 0;
  const isExpanded = expanded.has(unit.id);
  return <>
    <EntityListRow className={DESKTOP_COLUMNS} isLast={isLast}>
      <div className="flex min-w-0 items-center gap-2" style={{ paddingLeft: `${depth * 20}px` }}>
        <button type="button" className="grid size-7 shrink-0 place-items-center rounded hover:bg-[var(--color-accent)]" onClick={() => hasChildren && onToggle(unit.id)} aria-label={hasChildren ? `${isExpanded ? "Collapse" : "Expand"} ${unit.name}` : undefined}>
          {hasChildren ? isExpanded ? <ChevronDown className="size-4" /> : <ChevronRight className="size-4" /> : <span className="size-1.5 rounded-full bg-[var(--color-border)]" />}
        </button>
        <Building2 className="size-4 shrink-0 text-[var(--color-primary)]" />
        <div className="min-w-0">
          <span className="block truncate text-sm font-medium">{unit.name}</span>
          <span className="block truncate text-xs text-[var(--color-muted-foreground)]">{unit.description || "No description"}</span>
        </div>
      </div>
      <span className="font-mono text-xs text-[var(--color-muted-foreground)]">{unit.code}</span>
      <span className="truncate font-mono text-xs text-[var(--color-muted-foreground)]">{unit.referenceId || "—"}</span>
      <div className="flex items-center justify-end gap-1">
        <Button variant="ghost" size="icon-xs" onClick={() => onCreate(unit)} aria-label={`Add child unit to ${unit.name}`} title="Add child unit"><Plus className="size-3.5" /></Button>
        <Button variant="ghost" size="icon-xs" onClick={() => onEdit(unit)} aria-label={`Edit ${unit.name}`} title="Edit"><Pencil className="size-3.5" /></Button>
        <Button variant="ghost" size="icon-xs" onClick={() => onMove(unit)} aria-label={`Move ${unit.name}`} title="Move"><Move className="size-3.5" /></Button>
        <Button variant="ghost" size="icon-xs" disabled={deleting} onClick={() => onDelete(unit)} aria-label={`Delete ${unit.name}`} title="Delete"><Trash2 className="size-3.5 text-[var(--color-destructive)]" /></Button>
      </div>
    </EntityListRow>
    {hasChildren && isExpanded && unit.children.map((child, index) => <UnitRow key={child.id} unit={child} depth={depth + 1} isLast={isLast && index === unit.children.length - 1} expanded={expanded} onToggle={onToggle} onCreate={onCreate} onEdit={onEdit} onMove={onMove} onDelete={onDelete} deleting={deleting} />)}
  </>;
}

function UnitFormDialog({ open, onOpenChange, editing, parent, form, setForm, onSubmit, busy }: { open: boolean; onOpenChange: (open: boolean) => void; editing: UnitNode | null; parent: UnitNode | null; form: UnitForm; setForm: React.Dispatch<React.SetStateAction<UnitForm>>; onSubmit: (event: FormEvent) => void; busy: boolean }) {
  return <Dialog open={open} onOpenChange={onOpenChange}><DialogContent><form onSubmit={onSubmit}><DialogHeader><DialogTitle>{editing ? "Edit organization unit" : parent ? `New unit under ${parent.name}` : "New root organization unit"}</DialogTitle><DialogDescription>{editing ? "Update the unit's identifying details." : "Name and code are required."}</DialogDescription></DialogHeader><DialogBody className="space-y-4"><div className="space-y-2"><Label htmlFor="unit-name">Name</Label><Input id="unit-name" value={form.name} onChange={(event) => setForm((value) => ({ ...value, name: event.target.value }))} required autoFocus /></div><div className="space-y-2"><Label htmlFor="unit-code">Code</Label><Input id="unit-code" value={form.code} onChange={(event) => setForm((value) => ({ ...value, code: event.target.value }))} required /></div><div className="space-y-2"><Label htmlFor="unit-description">Description</Label><textarea id="unit-description" value={form.description} onChange={(event) => setForm((value) => ({ ...value, description: event.target.value }))} className="min-h-24 w-full rounded-md border border-[var(--color-input)] bg-transparent px-3 py-2 text-sm" /></div></DialogBody><DialogFooter><DialogClose asChild><Button type="button" variant="outline">Cancel</Button></DialogClose><Button type="submit" disabled={busy}>{busy ? "Saving…" : editing ? "Save changes" : "Create unit"}</Button></DialogFooter></form></DialogContent></Dialog>;
}

function MoveUnitDialog({ open, onOpenChange, moving, candidates, parentId, setParentId, onSubmit, busy }: { open: boolean; onOpenChange: (open: boolean) => void; moving: UnitNode | null; candidates: UnitNode[]; parentId: string; setParentId: (value: string) => void; onSubmit: (event: FormEvent) => void; busy: boolean }) {
  return <Dialog open={open} onOpenChange={onOpenChange}><DialogContent><form onSubmit={onSubmit}><DialogHeader><DialogTitle>Move organization unit</DialogTitle><DialogDescription>Choose a new parent, or make {moving?.name ?? "this unit"} a root unit.</DialogDescription></DialogHeader><DialogBody><Label htmlFor="new-parent">New parent</Label><select id="new-parent" value={parentId} onChange={(event) => setParentId(event.target.value)} className="mt-2 h-9 w-full rounded-md border border-[var(--color-input)] bg-transparent px-3 text-sm"><option value="">Root organization unit</option>{candidates.map((unit) => <option key={unit.id} value={unit.id}>{unit.name} ({unit.code})</option>)}</select></DialogBody><DialogFooter><DialogClose asChild><Button type="button" variant="outline">Cancel</Button></DialogClose><Button type="submit" disabled={busy}>{busy ? "Moving…" : "Move unit"}</Button></DialogFooter></form></DialogContent></Dialog>;
}
