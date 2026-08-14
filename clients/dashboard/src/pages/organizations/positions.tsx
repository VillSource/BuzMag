import { useEffect, useMemo, useState, type FormEvent } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Briefcase, Pencil, Plus, Trash2 } from "lucide-react";
import { toast } from "sonner";
import { listOrganizations } from "@/api/organizations";
import {
  createPosition,
  deletePosition,
  listPositions,
  updatePosition,
  type PositionDto,
  type PositionInput,
} from "@/api/positions";
import { Button } from "@/components/ui/button";
import {
  EntityEmpty,
  EntityListCard,
  EntityListHeader,
  EntityListLoading,
  EntityListRow,
  EntityPageHeader,
} from "@/components/list";
import {
  Dialog,
  DialogBody,
  DialogClose,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";

type PositionForm = { name: string; code: string; description: string };
const EMPTY_FORM: PositionForm = { name: "", code: "", description: "" };
const STORAGE_KEY = "selected_organization_id";
const DESKTOP_COLUMNS = "grid-cols-[minmax(200px,1.2fr)_120px_minmax(180px,1.5fr)_140px_100px]";

export function PositionsPage() {
  const client = useQueryClient();

  const orgsQuery = useQuery({
    queryKey: ["organizations"],
    queryFn: listOrganizations,
  });

  const orgs = orgsQuery.data ?? [];

  const [selectedOrgId, setSelectedOrgId] = useState<string | null>(() => {
    return localStorage.getItem(STORAGE_KEY);
  });

  useEffect(() => {
    if (orgs.length > 0) {
      const savedId = localStorage.getItem(STORAGE_KEY);
      const savedOrg = savedId ? orgs.find((o) => o.referenceId === savedId) : undefined;
      if (savedOrg) {
        if (selectedOrgId !== savedOrg.referenceId) {
          setSelectedOrgId(savedOrg.referenceId);
        }
      } else {
        const defaultOrg = orgs.find((o) => o.isDefault) ?? orgs[0];
        if (defaultOrg && selectedOrgId !== defaultOrg.referenceId) {
          setSelectedOrgId(defaultOrg.referenceId);
          localStorage.setItem(STORAGE_KEY, defaultOrg.referenceId);
        }
      }
    }
  }, [orgs, selectedOrgId]);

  const handleOrgChange = (newOrgId: string) => {
    setSelectedOrgId(newOrgId);
    if (newOrgId) {
      localStorage.setItem(STORAGE_KEY, newOrgId);
    } else {
      localStorage.removeItem(STORAGE_KEY);
    }
  };

  const queryKey = useMemo(() => ["organizations", "positions", selectedOrgId] as const, [selectedOrgId]);
  const query = useQuery({
    queryKey,
    queryFn: () => listPositions(selectedOrgId),
    enabled: selectedOrgId !== null || orgsQuery.isSuccess,
  });

  const positions = query.data ?? [];
  const [formOpen, setFormOpen] = useState(false);
  const [editing, setEditing] = useState<PositionDto | null>(null);
  const [form, setForm] = useState<PositionForm>(EMPTY_FORM);

  const refresh = () => client.invalidateQueries({ queryKey });

  const create = useMutation({
    mutationFn: createPosition,
    onSuccess: () => {
      toast.success("Position created");
      setFormOpen(false);
      refresh();
    },
    onError: () => toast.error("Could not create position"),
  });

  const update = useMutation({
    mutationFn: ({ referenceId, input }: { referenceId: string; input: PositionInput }) =>
      updatePosition(referenceId, input),
    onSuccess: () => {
      toast.success("Position updated");
      setFormOpen(false);
      refresh();
    },
    onError: () => toast.error("Could not update position"),
  });

  const remove = useMutation({
    mutationFn: deletePosition,
    onSuccess: () => {
      toast.success("Position deleted");
      refresh();
    },
    onError: () => toast.error("Could not delete position"),
  });

  const openCreate = () => {
    setEditing(null);
    setForm(EMPTY_FORM);
    setFormOpen(true);
  };

  const openEdit = (position: PositionDto) => {
    setEditing(position);
    setForm({ name: position.name, code: position.code, description: position.description ?? "" });
    setFormOpen(true);
  };

  const submitForm = (event: FormEvent) => {
    event.preventDefault();
    const input = {
      name: form.name.trim(),
      code: form.code.trim(),
      description: form.description.trim() || null,
    };
    if (!input.name || !input.code) return;

    if (editing) {
      update.mutate({ referenceId: editing.referenceId, input });
    } else {
      create.mutate({ ...input, organizationId: selectedOrgId });
    }
  };

  return (
    <div className="space-y-4 sm:space-y-6">
      <EntityPageHeader
        icon={Briefcase}
        title="Positions"
        total={query.data ? positions.length : null}
        unit="position"
        description="Manage job positions and roles within your organization."
      >
        <div className="flex flex-wrap items-center gap-3">
          {orgs.length > 0 && (
            <div className="flex items-center gap-2">
              <Label
                htmlFor="org-select"
                className="text-xs font-medium text-[var(--color-muted-foreground)] whitespace-nowrap"
              >
                Organization:
              </Label>
              <select
                id="org-select"
                value={selectedOrgId ?? ""}
                onChange={(e) => handleOrgChange(e.target.value)}
                className="h-9 rounded-md border border-[var(--color-input)] bg-[var(--color-card)] px-3 text-sm font-medium shadow-sm transition-colors focus:outline-none focus:ring-2 focus:ring-[var(--color-ring)]"
              >
                {orgs.map((org) => (
                  <option key={org.referenceId} value={org.referenceId}>
                    {org.referenceId} {org.isDefault ? "(Default)" : ""}
                  </option>
                ))}
              </select>
            </div>
          )}
          <Button onClick={openCreate}>
            <Plus className="size-4" />
            New position
          </Button>
        </div>
      </EntityPageHeader>

      {query.isLoading ? (
        <EntityListLoading rows={6} desktopColumns={DESKTOP_COLUMNS} />
      ) : positions.length === 0 ? (
        <EntityEmpty
          icon={Briefcase}
          title="No positions yet"
          body="Create positions to define roles in your organization structure."
          action={
            <Button onClick={openCreate}>
              <Plus className="size-4" />
              New position
            </Button>
          }
        />
      ) : (
        <EntityListCard>
          <EntityListHeader className={DESKTOP_COLUMNS}>
            <span>Position name</span>
            <span>Code</span>
            <span>Description</span>
            <span>Reference ID</span>
            <span className="text-right">Actions</span>
          </EntityListHeader>
          {positions.map((pos, index) => (
            <EntityListRow key={pos.referenceId} className={DESKTOP_COLUMNS} isLast={index === positions.length - 1}>
              <div className="flex min-w-0 items-center gap-2">
                <Briefcase className="size-4 shrink-0 text-[var(--color-primary)]" />
                <span className="truncate text-sm font-medium">{pos.name}</span>
              </div>
              <span className="font-mono text-xs text-[var(--color-muted-foreground)]">{pos.code}</span>
              <span className="truncate text-xs text-[var(--color-muted-foreground)]">
                {pos.description || "—"}
              </span>
              <span className="truncate font-mono text-xs text-[var(--color-muted-foreground)]">
                {pos.referenceId}
              </span>
              <div className="flex items-center justify-end gap-1">
                <Button
                  variant="ghost"
                  size="icon-xs"
                  onClick={() => openEdit(pos)}
                  aria-label={`Edit ${pos.name}`}
                  title="Edit"
                >
                  <Pencil className="size-3.5" />
                </Button>
                <Button
                  variant="ghost"
                  size="icon-xs"
                  disabled={remove.isPending}
                  onClick={() => {
                    if (window.confirm(`Delete position ${pos.name}? This cannot be undone.`)) {
                      remove.mutate(pos.referenceId);
                    }
                  }}
                  aria-label={`Delete ${pos.name}`}
                  title="Delete"
                >
                  <Trash2 className="size-3.5 text-[var(--color-destructive)]" />
                </Button>
              </div>
            </EntityListRow>
          ))}
        </EntityListCard>
      )}

      {query.isError && (
        <p role="alert" className="text-sm text-[var(--color-destructive)]">
          Positions could not be loaded.
        </p>
      )}

      <PositionFormDialog
        open={formOpen}
        onOpenChange={setFormOpen}
        editing={editing}
        form={form}
        setForm={setForm}
        onSubmit={submitForm}
        busy={create.isPending || update.isPending}
      />
    </div>
  );
}

function PositionFormDialog({
  open,
  onOpenChange,
  editing,
  form,
  setForm,
  onSubmit,
  busy,
}: {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  editing: PositionDto | null;
  form: PositionForm;
  setForm: React.Dispatch<React.SetStateAction<PositionForm>>;
  onSubmit: (event: FormEvent) => void;
  busy: boolean;
}) {
  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <form onSubmit={onSubmit}>
          <DialogHeader>
            <DialogTitle>{editing ? "Edit position" : "New position"}</DialogTitle>
            <DialogDescription>
              {editing ? "Update position code, name, and description." : "Position name and code are required."}
            </DialogDescription>
          </DialogHeader>
          <DialogBody className="space-y-4">
            <div className="space-y-2">
              <Label htmlFor="position-name">Name</Label>
              <Input
                id="position-name"
                value={form.name}
                onChange={(event) => setForm((value) => ({ ...value, name: event.target.value }))}
                required
                autoFocus
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="position-code">Code</Label>
              <Input
                id="position-code"
                value={form.code}
                onChange={(event) => setForm((value) => ({ ...value, code: event.target.value }))}
                required
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="position-description">Description</Label>
              <textarea
                id="position-description"
                value={form.description}
                onChange={(event) => setForm((value) => ({ ...value, description: event.target.value }))}
                className="min-h-24 w-full rounded-md border border-[var(--color-input)] bg-transparent px-3 py-2 text-sm"
              />
            </div>
          </DialogBody>
          <DialogFooter>
            <DialogClose asChild>
              <Button type="button" variant="outline">
                Cancel
              </Button>
            </DialogClose>
            <Button type="submit" disabled={busy}>
              {busy ? "Saving…" : editing ? "Save changes" : "Create position"}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
