CREATE TABLE todo_items (
    id bigint primary KEY GENERATED BY DEFAULT AS IDENTITY,
    name text NOT NULL,
    is_complete boolean NOT NULL DEFAULT FALSE
);

