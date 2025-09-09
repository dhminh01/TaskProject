import { render, screen, fireEvent } from "@testing-library/react";
import { describe, it, expect, vi } from "vitest";
import { CreateTaskForm } from "../../components/form/CreateTaskForm";
import { BrowserRouter } from "react-router-dom";
import { ApolloClient, InMemoryCache } from "@apollo/client";
import { ApolloProvider } from "@apollo/client/react";
import { HttpLink } from "@apollo/client";

// Create a mock Apollo Client
const mockClient = new ApolloClient({
  link: new HttpLink({ uri: "http://localhost:5000/tasks" }),
  cache: new InMemoryCache(),
  defaultOptions: {
    watchQuery: { fetchPolicy: "no-cache" },
    query: { fetchPolicy: "no-cache" },
  },
});

// Mock Apollo hooks
vi.mock("@apollo/client/react", () => {
  const mockUseMutation = () =>
    [
      vi.fn().mockResolvedValue({ data: { createTask: { id: "1" } } }),
      { loading: false },
    ] as const;

  const mockApolloProvider = ({ children }: { children: React.ReactNode }) =>
    children;

  return {
    useMutation: mockUseMutation,
    ApolloProvider: mockApolloProvider,
  };
});

const renderWithProviders = (component: React.ReactNode) => {
  return render(
    <BrowserRouter>
      <ApolloProvider client={mockClient}>{component}</ApolloProvider>
    </BrowserRouter>
  );
};

describe("CreateTaskForm", () => {
  it("renders form fields", () => {
    renderWithProviders(<CreateTaskForm />);
    expect(screen.getByLabelText(/title/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/description/i)).toBeInTheDocument();
  });

  it("validates required fields", async () => {
    renderWithProviders(<CreateTaskForm />);
    const submitButton = screen.getByRole("button", { name: /create/i });

    fireEvent.click(submitButton);

    expect(
      await screen.findByText(/Please enter a title/i)
    ).toBeInTheDocument();
  });

  it("allows entering task details", () => {
    renderWithProviders(<CreateTaskForm />);
    const titleInput = screen.getByLabelText(/title/i);
    const descriptionInput = screen.getByLabelText(/description/i);

    fireEvent.change(titleInput, { target: { value: "New Task" } });
    fireEvent.change(descriptionInput, {
      target: { value: "Task Description" },
    });

    expect(titleInput).toHaveValue("New Task");
    expect(descriptionInput).toHaveValue("Task Description");
  });
});
